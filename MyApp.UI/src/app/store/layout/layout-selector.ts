import { createFeatureSelector, createSelector } from '@ngrx/store'
import { LayoutState } from './layout-reducer'

export const getLayoutState = createFeatureSelector<LayoutState>('layout')

export const getLayout = createSelector(
  getLayoutState,
  (state: LayoutState) => state.LAYOUT
)

export const getLayoutColor = createSelector(
  getLayoutState,
  (state: LayoutState) => state.LAYOUT_THEME
)

export const getLayoutmode = createSelector(
  getLayoutState,
  (state: LayoutState) => state.LAYOUT_MODE
)

export const getLayoutTopbarColor = createSelector(
  getLayoutState,
  (state: LayoutState) => state.LAYOUT_TOPBAR_COLOR
)

export const getLayoutMenuColor = createSelector(
  getLayoutState,
  (state: LayoutState) => state.LAYOUT_MENU_COLOR
)

export const getLayoutMenuSize = createSelector(
  getLayoutState,
  (state: LayoutState) => state.LAYOUT_MENU_SIZE
)

export const getLayoutPosition = createSelector(
  getLayoutState,
  (state: LayoutState) => state.LAYOUT_POSITION
)
