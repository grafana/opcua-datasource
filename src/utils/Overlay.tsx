import * as React from 'react';

export function renderOverlay(
  bg: string,
  shallDisplayOverlay: () => boolean,
  onOverlayClicked: () => void
): JSX.Element {
  if (shallDisplayOverlay()) {
    return (
      <div
        style={{
          backgroundColor: bg,
          height: '100%',
          left: 0,
          opacity: 0.7,
          position: 'fixed',
          top: 0,
          width: '100%',
          zIndex: 5,
        }}
        onClick={(e) => onOverlayClicked()}
      />
    );
  }
  return <></>;
}
