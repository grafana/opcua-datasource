import React from 'react';
//import { SegmentAsync } from '@grafana/ui';

// const AddButton = (
//   <a className="gf-form-label query-part">
//     <i className="fa fa-plus" />
//   </a>
// );

export const SegmentLabel = ({ label, marginLeft }: any) => (
  <>
    <span style={marginLeft ? { marginLeft: '4px' } : {}} className="gf-form-label query-keyword">
      {label}
    </span>
  </>
);

//<SegmentAsync Component={AddButton} onChange={onChange} loadOptions={loadOptions} />
export const SegmentFrame = ({ label, onChange, loadOptions, children }: any) => (
  <>
    <div className="gf-form-inline">
      <div className="gf-form">
        <SegmentLabel label={label} />
      </div>
      {children}
    </div>
  </>
);
