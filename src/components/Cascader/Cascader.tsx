import React from 'react';
import { Icon, Select, Input } from '@grafana/ui';
import RCCascader from '../../../node_modules/rc-cascader/lib';
import { SelectableValue } from '@grafana/data';
import { css } from '@emotion/css';
import { onChangeCascader, onLoadDataCascader } from './optionMappings';

interface CascaderProps {
  /** The separator between levels in the search */
  separator?: string;
  placeholder?: string;
  options?: CascaderOption[];
  loadData?: (selectedOptions: CascaderOption[]) => void;
  onChange: (selected: string[], selectedOptions: CascaderOption[]) => void;
  onSelect(val: string): void;
  /** Sets the width to a multiple of 8px. Should only be used with inline forms. Setting width of the container is preferred in other cases.*/
  width?: number;
  initialValue?: string;
  allowCustomValue?: boolean;
  onSelectChange: boolean;
  /** A function for formatting the message for custom value creation. Only applies when allowCustomValue is set to true*/
  formatCreateLabel?: (val: string) => string;
}

interface CascaderState {
  isSearching: boolean;
  //searchableOptions: Array<SelectableValue<string[]>>;
  focusCascade: boolean;
  //Array for cascade navigation
  //rcValue: SelectableValue<string[]>;
  //activeLabel: string;
}

export interface CascaderOption {
  /**
   *  The value used under the hood
   */
  value: any;
  /**
   *  The label to display in the UI
   */
  label: string;
  /** Items will be just flattened into the main list of items recursively. */
  items?: CascaderOption[];
  disabled?: boolean;
  /** Avoid using */
  title?: string;
  /**  Children will be shown in a submenu. Use 'items' instead, as 'children' exist to ensure backwards compatibility.*/
  children?: CascaderOption[];
  isLeaf?: boolean;
}

const disableDivFocus = css(`
&:focus{
  outline: none;
}
`);

export class Cascader extends React.PureComponent<CascaderProps, CascaderState> {
  constructor(props: CascaderProps) {
    super(props);
    //const searchableOptions = this.flattenOptions(props.options);
    //const { rcValue, activeLabel } = this.setInitialValue(searchableOptions, props.initialValue);
    this.state = {
      isSearching: false,
      focusCascade: false,
      // searchableOptions,
      // rcValue,
      // activeLabel,
    };
  }

  flattenOptions = (options: CascaderOption[], optionPath: CascaderOption[] = []) => {
    let selectOptions: Array<SelectableValue<string[]>> = [];
    for (const option of options) {
      const cpy = [...optionPath];
      cpy.push(option);
      if (!option.items) {
        selectOptions.push({
          singleLabel: cpy[cpy.length - 1].label,
          label: cpy.map((o) => o.label).join(this.props.separator || ' / '),
          //value: cpy.map(o => o.value),
        });
      } else {
        selectOptions = [...selectOptions, ...this.flattenOptions(option.items, cpy)];
      }
    }
    return selectOptions;
  };

  setInitialValue(searchableOptions: Array<SelectableValue<string[]>>, initValue?: string) {
    if (!initValue) {
      return { rcValue: [], activeLabel: '' };
    }
    for (const option of searchableOptions) {
      const optionPath = option.value || [];

      if (optionPath.indexOf(initValue) === optionPath.length - 1) {
        return {
          rcValue: optionPath,
          activeLabel: option.singleLabel || '',
        };
      }
    }
    if (this.props.allowCustomValue) {
      return { rcValue: [], activeLabel: initValue };
    }
    return { rcValue: [], activeLabel: '' };
  }

  //For rc-cascader
  onChange = (value: string[], selectedOptions: CascaderOption[]) => {
    this.setState({
      //rcValue: value,
      focusCascade: true,
      //activeLabel: selectedOptions[selectedOptions.length - 1].label,
    });

    this.props.onChange(value, selectedOptions);
    this.props.onSelect(selectedOptions[selectedOptions.length - 1].value);
  };

  //For select
  onSelect = (obj: SelectableValue<string[]>) => {
    const valueArray = obj.value || [];
    this.setState({
      //activeLabel: obj.singleLabel || '',
      //rcValue: valueArray,
      isSearching: false,
    });
    this.props.onSelect(valueArray[valueArray.length - 1]);
  };

  onCreateOption = (value: string) => {
    this.setState({
      //activeLabel: value,
      //rcValue: [],
      isSearching: false,
    });
    this.props.onSelect(value);
  };

  onBlur = () => {
    this.setState({
      isSearching: false,
      focusCascade: false,
    });

    // if (this.state.activeLabel === '') {
    //   this.setState({
    //     rcValue: [],
    //   });
    // }
  };

  onBlurCascade = () => {
    this.setState({
      focusCascade: false,
    });
  };

  onInputKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (
      e.key === 'ArrowDown' ||
      e.key === 'ArrowUp' ||
      e.key === 'Enter' ||
      e.key === 'ArrowLeft' ||
      e.key === 'ArrowRight'
    ) {
      return;
    }
    this.setState({
      focusCascade: false,
      isSearching: true,
    });
  };

  render() {
    const { allowCustomValue, placeholder, width, loadData } = this.props;
    //const { focusCascade, isSearching, searchableOptions, rcValue, activeLabel } = this.state;
    const { focusCascade, isSearching } = this.state;

    return (
      <div>
        {isSearching ? (
          <Select
            allowCustomValue={allowCustomValue}
            placeholder={placeholder}
            autoFocus={!focusCascade}
            onChange={this.onSelect}
            onBlur={this.onBlur}
            //options={searchableOptions}
            onCreateOption={this.onCreateOption}
            formatCreateLabel={this.props.formatCreateLabel}
            width={width}
          />
        ) : (
          <RCCascader
            onChange={onChangeCascader(this.onChange)}
            options={this.props.options}
            loadData={onLoadDataCascader(loadData)}
            changeOnSelect={true}
            //value={rcValue.value}
            fieldNames={{ label: 'label', value: 'value', children: 'items' }}
            expandIcon={null}
            // Required, otherwise the portal that the popup is shown in will render under other components
            popupClassName={css`
              z-index: 9999;
            `}
          >
            <div className={disableDivFocus}>
              <Input
                width={width}
                placeholder={placeholder}
                onBlur={this.onBlurCascade}
                //value={activeLabel}
                onKeyDown={this.onInputKeyDown}
                onChange={this.onSelect}
                suffix={
                  focusCascade ? (
                    <Icon name="angle-up" />
                  ) : (
                    <Icon name="angle-down" style={{ marginBottom: 0, marginLeft: '4px' }} />
                  )
                }
              />
            </div>
          </RCCascader>
        )}
      </div>
    );
  }
}
