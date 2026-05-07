import { Injectable, TemplateRef } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Injectable({
  providedIn: 'root'
})
export class ModalService {

  constructor(private modalService: NgbModal) {}

  openRightModal(content: TemplateRef<any>, data?: any) {

    const modalRef = this.modalService.open(content, {
      size: 'xl',
      backdrop: 'static',
      windowClass: 'right-modal', // ✅ important
      animation: true
    });

    if (data) {
      modalRef.componentInstance.data = data;
    }

    return modalRef;
  }
}