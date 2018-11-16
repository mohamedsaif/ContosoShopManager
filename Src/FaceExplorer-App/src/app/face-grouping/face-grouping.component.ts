import { Component, OnInit } from '@angular/core';
import * as _ from 'lodash';
import { FaceApiService } from '../services/face-api-service.service';
import { forkJoin } from 'rxjs/observable/forkJoin';

@Component({
  selector: 'app-face-grouping',
  templateUrl: './face-grouping.component.html',
  styleUrls: ['./face-grouping.component.css']
})
export class FaceGroupingComponent implements OnInit {
  public imageUrls: string[];
  public faces: any[];
  public groupingResults: any = {};
  public loading = false;

  constructor(private faceApi: FaceApiService) { }

  ngOnInit() { }

  executeGrouping() {
    let urls = _.split(this.imageUrls, '\n'); 

    let detectList = [];
    _.forEach(urls, url => {
      if (url){
        detectList.push(this.faceApi.detect(url));
      }
    });

    this.loading = true;
    forkJoin(detectList).subscribe(detectResults => {
      this.faces = [];
      _.forEach(detectResults, (value, index) => this.faces.push({ url: urls[index], faceId: value[0].faceId} ));
      let faceIds = _.map(this.faces, 'faceId');

      this.faceApi.group(faceIds).subscribe(data => {
        this.groupingResults = data;
        this.loading = false;
      });
    });
  }

  getUrlForFace(faceId) {
    var face = _.find(this.faces, { faceId: faceId });
    return face.url;
  }

}
