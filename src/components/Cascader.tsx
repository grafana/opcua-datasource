import React from 'react';
import { Select, Input } from '@grafana/ui';
import { FormInputSize } from '@grafana/ui/components/Forms/types';
// @ts-ignore
import RCCascader from 'rc-cascader';

import { SelectableValue } from '@grafana/data';
import { css } from 'emotion';

interface CascaderProps<T> {
  separator?: string;
  loadData(val?: T): Promise<Array<CascaderOption<T>>>;
  onSelect(val: T[], selectedOptions: Array<CascaderOption<T>>): void;
  onCascadeClose?(): void;
  size?: FormInputSize;
  initialValue?: string;
}

interface CascaderState<T> {
  options: Array<CascaderOption<T>>;
  isSearching: boolean;
  searchableOptions: Array<SelectableValue<string[]>>;
  focusCascade: boolean;
  //Array for cascade navigation
  rcValue: SelectableValue<T[]>;
  activeLabel: string;
}

export interface CascaderOption<T> {
  value: T;
  label: string;
  // Items will be just flattened into the main list of items recursively.
  items?: Array<CascaderOption<T>>;
  disabled?: boolean;
  title?: string;
  // Children will be shown in a submenu.
  children?: Array<CascaderOption<T>>;
}

const disableDivFocus = css(`
&:focus{
  outline: none;
}
`);

export class Cascader<T> extends React.PureComponent<CascaderProps<T>, CascaderState<T>> {
  constructor(props: CascaderProps<T>) {
    super(props);

    const searchableOptions: Array<SelectableValue<string[]>> = [];

    console.log('props', props);
    const { rcValue, activeLabel } = this.setInitialValue(searchableOptions, props.initialValue);
    this.state = {
      options: [],
      isSearching: false,
      focusCascade: false,
      searchableOptions,
      rcValue,
      activeLabel,
    };
  }

  flattenOptions = (options: Array<CascaderOption<T>>, optionPath: Array<CascaderOption<T>> = []) => {
    let selectOptions: Array<SelectableValue<T[]>> = [];
    for (const option of options) {
      const cpy = [...optionPath];
      cpy.push(option);
      if (!option.items) {
        selectOptions.push({
          label: cpy.map(o => o.label).join(this.props.separator || ' / '),
          value: cpy.map(o => o.value),
        });
      } else {
        selectOptions = [...selectOptions, ...this.flattenOptions(option.items, cpy)];
      }
    }
    return selectOptions;
  };

  setInitialValue(searchableOptions: Array<SelectableValue<string[]>>, initValue?: string) {
    if (typeof initValue === "undefined") {
      return { rcValue: [], activeLabel: '' };
    }

    this.props.loadData().then((options: Array<CascaderOption<T>>) => {
      this.setState({ options });
    });
    // for (const option of searchableOptions) {
    //   const optionPath = option.value || [];

    //   if (optionPath.indexOf(initValue) === optionPath.length - 1) {
    //     return {
    //       rcValue: optionPath,
    //       activeLabel: option.label || '',
    //     };
    //   }
    // }
    return { rcValue: [], activeLabel: initValue };
  }

  //For rc-cascader
  onChange = (value: T[], selectedOptions: Array<CascaderOption<T>>) => {
    const { options } = this.state;
    const option = selectedOptions[selectedOptions.length - 1];
    const indexes: number[] = [];
    for (let i = 0, o: Array<CascaderOption<T>> = options; i < value.length; i++) {
      const index = o.findIndex(curr => curr.value === value[i]);
      if (index >= 0 && o[index].items) {
        o = o[index].items || [];
        indexes.push(index);
      }
    }

    if (indexes.length === value.length) {
      this.props.loadData(option.value).then((items: Array<CascaderOption<T>>) => {
        let o: Array<CascaderOption<T>> = options;
        indexes.forEach((optionsIndex, index) => {
          if (index === indexes.length - 1) {
            // Last index. update results
            o[optionsIndex] = {
              ...o[optionsIndex],
              items,
            };
          }
          o = o[optionsIndex].items || [];
        });
        this.setState({
          options,
          rcValue: value,
          activeLabel: selectedOptions.map(o => o.label).join(this.props.separator || ' / '),
        });
      });

      this.props.onSelect(value, selectedOptions);
    }
  };

  //For select
  onSelect = (obj: SelectableValue<string[]>) => {
    console.log('onSelect obj', obj);
    // const i: number = cascaderData.findIndex(item => item.value === nodeId);
    // if (i >= 0) {
    //   let results: OpcUaBrowseResults[] = await this.props.datasource.browse(nodeId);
    //   console.log("results", results, "i", i);
    //   cascaderData[i].items = results.map((item: OpcUaBrowseResults) => {
    //     return {
    //       label: item.displayName,
    //       value: item.nodeId,
    //       title: `title: ${item.displayName}`,
    //       items: [loadingOption],
    //     }
    //   })
    //   this.updateCascaderData(cascaderData);
    // }

    this.setState({
      activeLabel: obj.label || '',
      rcValue: obj.value || [],
      isSearching: false,
    });
  };

  onClick = () => {
    this.setState({
      focusCascade: true,
    });
  };

  onCascaderVisibleChange = (isVisible: boolean) => {
    if (this.state.focusCascade && !isVisible && this.props.onCascadeClose) {
      this.props.onCascadeClose();
    }

    this.setState({
      focusCascade: isVisible,
    });
  };

  onBlur = () => {
    this.setState({
      isSearching: false,
      focusCascade: false,
    });

    if (this.state.activeLabel === '') {
      this.setState({
        rcValue: [],
      });
    }
  };

  onInputKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key !== 'ArrowDown' && e.key !== 'ArrowUp' && e.key !== 'Enter' && e.key !== 'ArrowLeft' && e.key !== 'ArrowRight') {
      this.setState({
        focusCascade: false,
        isSearching: true,
      });
      if (e.key === 'Backspace') {
        const label = this.state.activeLabel || '';
        this.setState({
          activeLabel: label.slice(0, -1),
        });
      }
    }
  };

  onInputChange = (value: string) => {
    this.setState({
      activeLabel: value,
    });
  };

  render() {
    const { size } = this.props;
    const { focusCascade, isSearching, searchableOptions, rcValue, activeLabel, options } = this.state;
    console.log('options', options);
    return (
      <div>
        {isSearching ? (
          <Select
            inputValue={activeLabel}
            placeholder="Search"
            autoFocus={!focusCascade}
            onChange={this.onSelect}
            onInputChange={this.onInputChange}
            onBlur={this.onBlur}
            options={searchableOptions}
            size={size || 'md'}
          />
        ) : (
          <RCCascader
            onChange={this.onChange}
            onClick={this.onClick}
            options={options}
            onPopupVisibleChange={this.onCascaderVisibleChange}
            value={rcValue}
            fieldNames={{ label: 'label', value: 'value', children: 'items' }}
            expandIcon={null}
            changeOnSelect
          >
            <div className={disableDivFocus}>
              <Input style={{ width: '500px' }} value={activeLabel} onKeyDown={this.onInputKeyDown} onChange={() => {}} />
            </div>
          </RCCascader>
        )}
      </div>
    );
  }
}
