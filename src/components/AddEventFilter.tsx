import React, { PureComponent } from 'react';
import { SegmentFrame } from './SegmentFrame';
import { FilterOperator, EventFilter, EventFilterOperatorUtil } from '../types';

export interface Props {
  add(filter: EventFilter): void;
}

type State = {
  eventFilter: EventFilter;
};

export class AddEventFilter extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      eventFilter: {
        oper: FilterOperator.GreaterThan,
        operands: ['Severity', '500'],
      },
    };
    this.handleSubmit = this.handleSubmit.bind(this);
    this.changeOperator = this.changeOperator.bind(this);
  }

  handleSubmit(event: { preventDefault: () => void }) {
    this.props.add(this.state.eventFilter);
    event.preventDefault();
  }

  changeOperator(event: { target: any }) {
    const target = event.target;
    const value = target.value as FilterOperator;
    switch (value) {
      case FilterOperator.GreaterThan:
      case FilterOperator.GreaterThanOrEqual:
      case FilterOperator.LessThan:
      case FilterOperator.LessThanOrEqual:
      case FilterOperator.Equals: {
        var eventFilter = {
          oper: value,
          operands: ['', ''],
        };
        this.setState({ eventFilter: eventFilter });
      }
    }
  }

  changeOperand(event: { target: any }, operandIdx: number) {
    const target = event.target;
    const value = target.value;

    var operands = this.state.eventFilter.operands.slice();
    operands[operandIdx] = value;
    var eventFilter = {
      oper: this.state.eventFilter.oper,
      operands: operands,
    };
    this.setState({ eventFilter: eventFilter });
  }

  renderDropdown() {
    return (
      <select onSelect={this.changeOperator}>
        {EventFilterOperatorUtil.operNames.map((n, idx) => {
<<<<<<< HEAD
          return (
            <option key={`option${idx}`} value={idx}>
              {n}
            </option>
          );
=======
          return <option value={idx}>{n}</option>;
>>>>>>> master
        })}
      </select>
    );
  }

  renderOperands(oper: FilterOperator) {
    switch (oper) {
      case FilterOperator.GreaterThan:
      case FilterOperator.GreaterThanOrEqual:
      case FilterOperator.LessThan:
      case FilterOperator.LessThanOrEqual:
      case FilterOperator.Equals:
        return (
          <>
            <SegmentFrame label="Event Field">
              <input
                name="browsename"
                type="input"
                value={this.state.eventFilter.operands[0]}
<<<<<<< HEAD
                onChange={(ev) => this.changeOperand(ev, 0)}
=======
                onChange={ev => this.changeOperand(ev, 0)}
>>>>>>> master
              />
            </SegmentFrame>
            <SegmentFrame label="Alias" marginLeft>
              <input
                name="alias"
                type="input"
                value={this.state.eventFilter.operands[1]}
<<<<<<< HEAD
                onChange={(ev) => this.changeOperand(ev, 1)}
=======
                onChange={ev => this.changeOperand(ev, 1)}
>>>>>>> master
              />
            </SegmentFrame>
          </>
        );
    }
    return <></>;
  }

  render() {
    return (
      <div>
        <br />
        <form onSubmit={this.handleSubmit}>
          {this.renderDropdown()}
          {this.renderOperands(this.state.eventFilter.oper)}
          <input type="submit" value="Add Filter" />
        </form>
      </div>
    );
  }
}
