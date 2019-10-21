package logger

import (
	hclog "github.com/hashicorp/go-hclog"
)


// Wrapper time
var Print hclog.Logger

// SetLog sets the log
func SetLog(log hclog.Logger) {
	Print = log
}