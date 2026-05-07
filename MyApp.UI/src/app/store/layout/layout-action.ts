import { createAction, props } from '@ngrx/store'

export const changelayout = createAction(
  '[Layout] Set Layout',
  props<{ layout: string }>()
)

export const changetheme = createAction(
  '[Layout] Set Color',
  props<{ color: string }>()
)

export const changeMode = createAction(
  '[Layout] Set Mode',
  props<{ mode: string }>()
)

export const changetoparcolor = createAction(
  '[Layout] Set Topbar Color',
  props<{ topbarcolor: string }>()
)

export const changemenucolor = createAction(
  '[Layout] Set Menu Color',
  props<{ menucolor: string }>()
)

export const changemenusize = createAction(
  '[Layout] Set Menu Size',
  props<{ menusize: string }>()
)

export const changeposition = createAction(
  '[Layout] Set Position',
  props<{ position: string }>()
)

export const resetState = createAction('[App] Reset State')
