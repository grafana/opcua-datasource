import React from 'react';
export const SegmentLabel = ({ label, marginLeft, width }: any) => (
  <>
    <span style={marginLeft ? { marginLeft: '4px'} : {}} className={`gf-form-label query-keyword width-${width}`}>
      {label}
    </span>
  </>
);

//<SegmentAsync Component={AddButton} onChange={onChange} loadOptions={loadOptions} />
export const SegmentFrame = ({ label, onChange, loadOptions, width, children }: any) => (
  <div style={{ display: 'flex' }}>
    <div className="gf-form-inline">
      <div className="gf-form">
        <SegmentLabel label={label} width={width} />
      </div>
      {children}
    </div>
    <div className={'gf-form gf-form--grow'}>
      <div className={'gf-form-label gf-form-label--grow'}></div>
    </div>
  </div>
);

export const SegmentRow = ({ label, children }: any) => (
  <>
    <tr>
      <td>
        <div className="gf-form-label query-keyword">{label}</div>
      </td>
      <td>{children}</td>
    </tr>
  </>
);
