import { FormGroup, FormControl, Validators } from '@angular/forms';

/** Collect an answer; provider selection remains entirely on the API. */
export function updateRoofInstallationStep(steps: any[], form: FormGroup, field: string, value: string): void {
  if (field !== 'projectType' || !steps.some(s => s.field === 'roofType')) return;
  const existing = steps.findIndex(s => s.field === 'roofInstallationHomeType');
  if (existing >= 0) steps.splice(existing, 1);
  form.removeControl('roofInstallationHomeType');
  if (value !== 'Install New Roof') return;
  const project = steps.findIndex(s => s.field === 'projectType');
  steps.splice(project + 1, 0, {
    title: 'Is this roof for a newly constructed home or an existing home?',
    type: 'buttons', field: 'roofInstallationHomeType',
    options: ['Newly constructed home', 'Existing home']
  });
  form.addControl('roofInstallationHomeType', new FormControl('', Validators.required));
}