package cgoopc

import (
	"encoding/json"
	"errors"
	"fmt"
	"io/ioutil"
	"log"
	"strings"
	"sync"
	"time"
)

type cache map[string]string

var mapLock = &sync.RWMutex{}

const SELECTED_TAGS_PATH = "./config/selected-tags.json"
const ALIASES_PATH = "./config/aliases.json"

type LogLevel int

const (
	FATAL  LogLevel = 0
	ERROR  LogLevel = 1
	WARN   LogLevel = 3
	INFO   LogLevel = 4
	DEBUG1 LogLevel = 5
	DEBUG2 LogLevel = 6
)

type Config struct {
	Iface    string   `json:"iface"`
	Host     string   `json:"host"`
	Port     int      `json:"port"`
	Exchange string   `json:"exchange"`
	Hosts    []string `json:"hosts"`
	LogLevel LogLevel `json:"loglevel"`
	RootNode string   `json:"rootnode"`
	Origin   string   `json:"origin"`
}

type OPCUA_Driver struct {
	Client          *Client
	Tags            []TagItem
	LogLevel        LogLevel
	URL             string
	Error           chan int
	OnValueChange   func(tag TagItem)
	timeout         UA_UInt16
	selectedTagsMap tagMap
	ignoredTags     map[string]bool
	config          *Config
}

func New_OPC_Driver(conf *Config, url string, timeout uint16) *OPCUA_Driver {
	return &OPCUA_Driver{
		Client:          NewClient(conf.LogLevel),
		Tags:            []TagItem{},
		LogLevel:        conf.LogLevel,
		URL:             url,
		Error:           make(chan int, 2),
		OnValueChange:   nil,
		timeout:         UA_UInt16(timeout),
		selectedTagsMap: tagMap{},
		ignoredTags:     map[string]bool{},
		config:          conf,
	}
}

func (d *OPCUA_Driver) SetURL(url string) {
	d.URL = url
}

func (d *OPCUA_Driver) ConnectRetry() {
	log.Printf("[INFO] OPCUA driver LogLevel = %d", d.LogLevel)

	func(opcDriver *OPCUA_Driver) {
		for {
			if d.LogLevel >= INFO {
				log.Printf("[INFO] connecting to OPCUA server %s", d.URL)
			}
			conStatus := opcDriver.Client.Connect(d.URL)
			if conStatus != 0 {
				log.Printf("\n\n\n[ERROR] connection error, status=%d", conStatus)
				log.Printf("[INFO] pause 5s ...\n\n\n\n\n\n\n")
				time.Sleep(5 * time.Second)
			} else {
				log.Printf("[INFO] connected to OPCUA %s, status=%d", d.URL, conStatus)
				opcDriver.BrowseTags() //Tags = opcDriver.Client.BrowseTags()
				opcDriver.Restore()
				return
			}
		}
	}(d)
}

func (d *OPCUA_Driver) BrowseTags() {
	raw := d.Client.BrowseTags()

	filtered := make([]TagItem, 0, len(raw))

	tmp := map[string]int{}
	for _, t := range raw {
		hash := fmt.Sprintf("%d+%s", t.NameSpace, t.StringNodeId)
		if _, ok := tmp[hash]; !ok {
			tmp[hash] = 1

			// Extend tag with calculated values
			i := strings.Index(t.StringNodeId, d.config.RootNode)
			if i >= 0 {
				t.EncodedTagId = t.StringNodeId[i+len(d.config.RootNode):]
			} else {
				t.EncodedTagId = t.StringNodeId
			}

			filtered = append(filtered, t)
			if d.config.LogLevel >= 6 {
				log.Printf("[DEBUG] EncodedTagId=%s, stringId=%s\n", t.EncodedTagId, t.StringNodeId)
			}
		}
	}
	d.Tags = filtered
}

func (d *OPCUA_Driver) UnSubscribeAll() {
	log.Println("[DEBUG] UnSubscribeAll() stop listen OPCUA ...")
	d.Client.StopListen()
	status := d.Client.SubscriptionDelete()
	if status == UA_STATUSCODE_BADCONNECTIONCLOSED {
		d.Client.ReConnect(0)
	} else if status != UA_STATUSCODE_GOOD {
		d.Client.ReConnect(0)
	} else {
		log.Println("[DEBUG] UnSubscribeAll() ok, no reconnect")
	}
}

func (d *OPCUA_Driver) SubscribeSelected() error {
	var err error
	var subId = 0

	subId, err = d.Client.SubscriptionCreate(200.0)
	if err != nil {
		return err
	}

	if subId == 0 {
		return errors.New("Subscribe selected() error: No subscriptionId was found\n")
	} else {
		log.Printf("[INFO] subscriptionId=%d\n", subId)
	}

	for _, tag := range d.selectedTagsMap {
		subErr := d.Client.Subscribe(subId, uint16(tag.NameSpace), tag.StringNodeId, d.handlerOnChange)
		if subErr != nil {
			return subErr
		} else {
			log.Printf("[DEBUG] SubscribeSelected() subscribed to %s\n", tag.StringNodeId)
		}
	}
	d.Client.StartListen(100)
	return nil
}

func (d *OPCUA_Driver) UpdateEncodedAliases() {
	for ti, tag := range d.Tags {
		// Extend tag with calculated values
		i := strings.Index(tag.StringNodeId, d.config.RootNode)
		if i >= 0 {
			d.Tags[ti].EncodedTagId = tag.StringNodeId[i+len(d.config.RootNode):]
		} else {
			d.Tags[ti].EncodedTagId = tag.StringNodeId
		}
	}
}

func (d *OPCUA_Driver) UpdateAliases(changes []map[string]interface{}) {

	// help map of indexes to search in d.Tags
	tmpTagIndexMap := make(map[string]int, len(d.Tags))
	for i, t := range d.Tags {
		tmpTagIndexMap[t.StringNodeId] = i
	}

	for _, alias := range changes {
		var tag TagItem
		var ok bool
		var id string = alias["id"].(string)

		if tag, ok = d.selectedTagsMap[id]; !ok {
			// not found in selected, let's find in tags
			if index, ok := tmpTagIndexMap[id]; !ok {
				// unknown tad id, very strange!
				log.Printf("[ERROR] UpdateAliases() couldn't find id=%s in selected or tag\n", id)
				continue
			} else {
				tag = d.Tags[index]
			}
		} else {
			// found in selected, good
		}

		tag.HasCustomAlias = alias["hasCustomAlias"].(bool)
		tag.CustomAlias = alias["customAlias"].(string)

		d.selectedTagsMap[id] = tag

		for i, t := range d.Tags {
			if t.StringNodeId == id {
				d.Tags[i] = tag
				break
			}
		}

	}
}

func (d *OPCUA_Driver) handlerOnChange(monId uint32, val interface{}, status uint32) {
	tag := d.Client.sub.monitors[monId]
	tag.Data = val
	tag.Quality = status
	fulltag, ok := d.selectedTagsMap[tag.StringNodeId]
	if ok {
		fulltag.Data = tag.Data
		fulltag.Quality = tag.Quality
		d.OnValueChange(fulltag)
	}
	if d.LogLevel >= DEBUG1 {
		log.Printf("[INFO] OPCUA CHANGE EVENT monitoringId = %d, tag=%s, value = %d\n", monId, tag.StringNodeId, val)
	}
}

func (d *OPCUA_Driver) SetSelectedList(selected []string) {
	d.UnSubscribeAll()
	// clean map
	mapLock.Lock()
	for k := range d.selectedTagsMap {
		delete(d.selectedTagsMap, k)
	}
	mapLock.Unlock()

	tmpMap := map[string]TagItem{}
	for _, v := range d.Tags {
		if v.StringNodeId != "" {
			tmpMap[v.StringNodeId] = v
		}
	}
	for _, tagId := range selected {
		if t, ok := tmpMap[tagId]; ok {
			d.selectedTagsMap[tagId] = t
		}
	}
}

func (d *OPCUA_Driver) IsSelected(stringId string) (TagItem, bool) {
	t, ok := d.selectedTagsMap[stringId]
	return t, ok
}

func (d *OPCUA_Driver) Backup() {
	list := make([]string, len(d.selectedTagsMap), len(d.selectedTagsMap))
	aliases := make(map[string]string, 0)
	i := 0
	for k, tag := range d.selectedTagsMap {
		if tag.HasCustomAlias {
			aliases[k] = tag.CustomAlias
		}
		list[i] = k
		i++
	}

	byteAliases, err := json.Marshal(aliases)
	if err != nil {
		log.Printf("[ERROR] Failed to backup aliases %s", err)
		return
	}
	err = ioutil.WriteFile(ALIASES_PATH, byteAliases, 0644)
	if err != nil {
		log.Printf("[ERROR] Failed to backup aliases %s", err)
		return
	}

	byteObj, err := json.Marshal(list)
	if err != nil {
		log.Printf("[ERROR] Failed to backup selected tags %s", err)
		return
	}
	err = ioutil.WriteFile(SELECTED_TAGS_PATH, byteObj, 0644)
	if err != nil {
		log.Printf("[ERROR] Failed to backup selected tags %s", err)
		return
	}

	log.Printf("[INFO] Successfull backup %+s", string(byteObj))
}

func (d *OPCUA_Driver) Restore() {
	// read selected tags from file
	raw, err := ioutil.ReadFile(SELECTED_TAGS_PATH)
	if err != nil {
		log.Printf("[ERROR] Failed to restore %s", err)
		return
	}
	var list []string
	err = json.Unmarshal(raw, &list)
	if err != nil {
		log.Printf("[ERROR] Failed to restore selected tags %s", err)
		return
	}

	// read aliases from file
	bytesAliases, err := ioutil.ReadFile(ALIASES_PATH)
	if err != nil {
		log.Printf("[ERROR] Failed to restore aliases %s", err)
		return
	}
	var aliases map[string]string
	err = json.Unmarshal(bytesAliases, &aliases)
	if err != nil {
		log.Printf("[ERROR] Failed to restore %s", err)
		return
	}

	tmpMap := map[string]TagItem{}
	for _, tagItem := range d.Tags {
		if alias, ok := aliases[tagItem.StringNodeId]; ok {
			tagItem.HasCustomAlias = true
			tagItem.CustomAlias = alias
		}
		tmpMap[tagItem.StringNodeId] = tagItem

	}
	for _, tagId := range list {
		if v, ok := tmpMap[tagId]; ok {
			d.selectedTagsMap[tagId] = v
		}
	}
}

func (d *OPCUA_Driver) GetNodeIds(displayName string) (ns UA_UInt16, nodeStringId string, err error) {
	for _, item := range d.selectedTagsMap {
		if item.DisplayName == displayName {
			return item.NameSpace, item.StringNodeId, nil
		}
	}
	for _, item := range d.Tags {
		if item.DisplayName == displayName {
			return item.NameSpace, item.StringNodeId, nil
		}
	}
	return 0, "", errors.New(fmt.Sprintf("stringNodeId not found by displayName=%s", displayName))
}
