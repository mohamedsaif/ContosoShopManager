import { Component, OnInit } from '@angular/core';
import { FaceApiService } from '../services/face-api-service.service';
import { InputBoxService } from '../input-box/input-box.service';
import * as _ from 'lodash';
import { ToasterService } from 'angular2-toaster';

@Component({
  selector: 'app-configuration',
  templateUrl: './configuration.component.html',
  styleUrls: ['./configuration.component.css']
})
export class ConfigurationComponent implements OnInit {
  public loading = false;
  public personFaces = [];
  public personGroups = [];
  public personList = [];
  public selectedGroupId = '';
  public selectedPerson: any;

  constructor(private faceApi: FaceApiService, private inputBox: InputBoxService, private toastr: ToasterService) { }

  ngOnInit() {
    this.faceApi.getPersonGroups().subscribe(data => this.personGroups = data);
  }

  addPersonGroup(){
    this.inputBox.show('Add Person Group', 'Person Group Name:').then(result => { 
      let newPersonGroup = { personGroupId: _.kebabCase(result), name: result };
      this.faceApi.createPersonGroup(newPersonGroup).subscribe(data => {
        this.personGroups.push(newPersonGroup);
        this.selectedGroupId = newPersonGroup.personGroupId;
        this.onGroupsChange();
      });
    });
  }

  deletePersonGroup() {
    this.faceApi.deletePersonGroup(this.selectedGroupId).subscribe(() => {
      _.remove(this.personGroups, x => x.personGroupId === this.selectedGroupId);
      this.selectedGroupId = '';
    });
  }

  onGroupsChange() {
    if (this.selectedGroupId) {
      this.loading = true;
      this.faceApi.getPersonsByGroup(this.selectedGroupId).subscribe(data => {
        this.personList = data; 
        this.selectedPerson = null;
        this.personFaces = [];
        this.loading = false; 
      });
    }
  }

  personClick(person) {
    this.selectedPerson = person;
    this.faceApi.getPersonFaces(this.selectedGroupId, this.selectedPerson.personId).subscribe(data => { 
      this.personFaces = data;
    });
  }

  addPerson() {
    this.inputBox.show('Add Person', 'Person Name:').then(result => {
      let newPerson: any = { name: result };
      this.faceApi.createPerson(this.selectedGroupId, { name: result }).subscribe(data => {
        newPerson.personId = data.personId;
        this.personList.push(newPerson);
        this.selectedPerson = newPerson;
      });
    });
  }

  deletePerson(personId) {
    this.faceApi.deletePerson(this.selectedGroupId, this.selectedPerson.personId).subscribe(() => {
      _.remove(this.personList, x => x.personId === this.selectedPerson.personId);
      this.selectedPerson = null;
    });
  }

  addPersonFace() {
    this.inputBox.show('Add Face', 'URL:').then(result => {
      this.faceApi.addPersonFace(this.selectedGroupId, this.selectedPerson.personId, result).subscribe(data => {
        let newFace = { persistedFaceId: data.persistedFaceId, userData: result };
        this.personFaces.push(newFace);
      });
    });
  }

  deletePersonFace(persistedFaceId) {
    this.faceApi.deletePersonFace(this.selectedGroupId, this.selectedPerson.personId, persistedFaceId).subscribe(() => {
      _.remove(this.personFaces, x => x.persistedFaceId === persistedFaceId);
    });
  }

  trainPersonGroup() {
    this.loading = true;
    this.faceApi.trainPersonGroup(this.selectedGroupId).subscribe(() => {
      this.toastr.pop('info', 'Training Initiated', 'Training has been initiated...');
      this.loading = false;
    });
  }

  getGroupTrainingStatus() {
    this.loading = true;
    this.faceApi.getPersonGroupTrainingStatus(this.selectedGroupId).subscribe(result => {
      switch (result.status) {
        case 'succeeded':
          this.toastr.pop('success', 'Training Succeeded');
          break;
        case 'running':
          this.toastr.pop('info', 'Training still in progress...', 'Check back later');
          break;
        case 'failed':
          this.toastr.pop('error', 'Error during Training', result.message);
          break;
        default:
          break;
      }
      this.loading = false;
    });
  }

}