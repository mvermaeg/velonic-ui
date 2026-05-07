import { Action, createReducer, on } from '@ngrx/store'
import {
  changelayout,
  changemenucolor,
  changemenusize,
  changeMode,
  changeposition,
  changetheme,
  changetoparcolor,
  resetState,
} from './layout-action'
import {
  LAYOUT_COLOR_TYPES,
  LAYOUT_MENU_COLOR,
  LAYOUT_MENU_SIZE,
  LAYOUT_MODE_TYPES,
  LAYOUT_POSITION_TYPES,
  LAYOUT_TOPBAR_COLOR_TYPES,
  LAYOUT_TYPES,
} from './layout'

export interface LayoutState {
  LAYOUT: string
  LAYOUT_THEME: string
  LAYOUT_MODE: string
  LAYOUT_TOPBAR_COLOR: string
  LAYOUT_MENU_COLOR: string
  LAYOUT_MENU_SIZE: string
  LAYOUT_POSITION: string
}

// IntialState
export const initialState: LayoutState = {
  LAYOUT: LAYOUT_TYPES.VERTICAL,
  LAYOUT_THEME: LAYOUT_COLOR_TYPES.LIGHTMODE,
  LAYOUT_MODE: LAYOUT_MODE_TYPES.FLUID,
  LAYOUT_TOPBAR_COLOR: LAYOUT_TOPBAR_COLOR_TYPES.LIGHTMODE,
  LAYOUT_MENU_COLOR: LAYOUT_MENU_COLOR.DARKMODE,
  LAYOUT_MENU_SIZE: LAYOUT_MENU_SIZE.DEFAULT,
  LAYOUT_POSITION: LAYOUT_POSITION_TYPES.FIXED,
}

export const layoutReducer = createReducer(
  initialState,
  on(changelayout, (state, action) => ({ ...state, LAYOUT: action.layout })),
  on(changetheme, (state, action) => ({
    ...state,
    LAYOUT_THEME: action.color,
  })),
  on(changeMode, (state, action) => ({ ...state, LAYOUT_MODE: action.mode })),
  on(changetoparcolor, (state, action) => ({
    ...state,
    LAYOUT_TOPBAR_COLOR: action.topbarcolor,
  })),
  on(changemenucolor, (state, action) => ({
    ...state,
    LAYOUT_MENU_COLOR: action.menucolor,
  })),
  on(changemenusize, (state, action) => ({
    ...state,
    LAYOUT_MENU_SIZE: action.menusize,
  })),
  on(changeposition, (state, action) => ({
    ...state,
    LAYOUT_POSITION: action.position,
  })),
  on(resetState, () => initialState)
)

// Selector
export function reducer(state: LayoutState | undefined, action: Action) {
  return layoutReducer(state, action)
}
