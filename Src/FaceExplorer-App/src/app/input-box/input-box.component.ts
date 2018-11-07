import { Component, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-input-box',
  templateUrl: './input-box.component.html',
  styleUrls: ['./input-box.component.css']
})
export class InputBoxComponent implements OnInit {
  public properties: InputModalProperties;
  public inputValue = '';

  constructor(public activeModal: NgbActiveModal) { }

  ngOnInit() { }

  save() {
    this.activeModal.close(this.inputValue);
  }
}


export class InputModalProperties {
  title: string;
  message: string;
}