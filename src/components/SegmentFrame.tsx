import React from 'react';
import { SegmentAsync } from '@grafana/ui';

const AddButton = (
  <a className="gf-form-label query-part">
    <i className="fa fa-plus" />
  </a>
);

export const SegmentLabel = ({ label }: any) => (
  <>
    <span className="gf-form-label query-keyword">{label}</span>
  </>
);

export const SegmentFrame = ({ label, onChange, loadOptions, children }: any) => (
  <>
    <div className="gf-form-inline">
      <div className="gf-form">
        <SegmentLabel label={label} />
      </div>
      {children}
      <SegmentAsync Component={AddButton} onChange={onChange} loadOptions={loadOptions} />
    </div>
  </>
);
