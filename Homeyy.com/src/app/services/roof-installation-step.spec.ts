import { FormBuilder, FormGroup } from '@angular/forms';
import { updateRoofInstallationStep } from './roof-installation-step';

describe('Roof installation answer', () => {
  it('requires the home type for installation and clears it when changing project', () => {
    const steps: any[] = [{field: 'projectType'}, {field: 'roofType'}];
    const form: FormGroup = new FormBuilder().group({projectType: 'Install New Roof'});
    updateRoofInstallationStep(steps, form, 'projectType', 'Install New Roof');
    expect(steps[1].field).toBe('roofInstallationHomeType');
    expect(form.valid).toBeFalse();
    form.get('roofInstallationHomeType')!.setValue('Existing home');
    expect(form.valid).toBeTrue();
    updateRoofInstallationStep(steps, form, 'projectType', 'Install New Roof');
    expect(steps.filter(s => s.field === 'roofInstallationHomeType').length).toBe(1);
    expect(form.get('roofInstallationHomeType')!.value).toBe('');
    updateRoofInstallationStep(steps, form, 'projectType', 'Repair Roof');
    expect(form.contains('roofInstallationHomeType')).toBeFalse();
    expect(steps.length).toBe(2);
  });
});