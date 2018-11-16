import { Injectable } from '@angular/core';
import { InputBoxComponent, InputModalProperties } from './input-box.component';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Injectable()
export class InputBoxService {

  constructor(private modal: NgbModal) { }

  show(title: string, message: string) {
    let modalRef = this.modal.open(InputBoxComponent);
    modalRef.componentInstance.properties = <InputModalProperties>{ title: title, message: message };

    const promise = new Promise((resolve, reject) => {
      modalRef.result.then(result => resolve(result), reason => reason);
    });
    return promise;
  }

}
