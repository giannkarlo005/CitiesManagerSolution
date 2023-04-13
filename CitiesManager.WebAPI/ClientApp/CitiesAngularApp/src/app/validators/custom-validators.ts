import { AbstractControl, FormGroup, ValidationErrors } from "@angular/forms";

export function CompareValidator(controlToValidate: string, controlToCompare: string) {
  return (formGroupControl: AbstractControl): ValidationErrors | null => {
    const formGroup = formGroupControl as FormGroup;
    const control = formGroup.controls[controlToValidate];
    const matchingControl = formGroup.controls[controlToCompare];

    if (control.value !== matchingControl.value) {
      formGroup.get(controlToCompare)?.setErrors({ compareValidator: { valid: false } });
      return { compareValidator: true }
    }
    return null;
  };
}
