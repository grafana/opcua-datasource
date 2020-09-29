import React from 'react';
// @ts-ignore
import { useTheme } from '@grafana/ui';
import { GrafanaTheme } from '@grafana/data';

export interface ThemeGetterProps {
    onTheme(theme: GrafanaTheme) : void;
}


export const ThemeGetter: React.FC<ThemeGetterProps> = props => {
    const { onTheme } = props;
    const theme = useTheme();
    onTheme(theme);
    return (
        <></>
  );
};
