import { Component, OnInit } from '@angular/core';
import { FaceApiService } from '../services/face-api-service.service';
import * as _ from 'lodash';
import { forkJoin } from 'rxjs/observable/forkJoin';

@Component({
  selector: 'app-find-similar',
  templateUrl: './find-similar.component.html',
  styleUrls: ['./find-similar.component.css']
})
export class FindSimilarComponent implements OnInit {
  public faces: any[];
  public loading = false;
  public imageUrls: string[];
  public queryFace: string = 'https://psfaceapicourse.blob.core.windows.net/individuals/steve4.png';
  public findSimilarResults: any[];

  constructor(private faceApi: FaceApiService) { }

  ngOnInit() { }

  findSimilar() {
    this.loading = true;

    // 1. First create a face list with all the imageUrls
    let faceListId = (new Date()).getTime().toString(); // comically naive, but this is just for demo
    this.faceApi.createFaceList(faceListId).subscribe(() => {

      // 2. Now add all faces to face list
      let facesSubscribableList = [];
      let urls = _.split(this.imageUrls, '\n');
      _.forEach(urls, url => {
        if (url) {
          facesSubscribableList.push(this.faceApi.addFace(faceListId, url));
        }
      });

      forkJoin(facesSubscribableList).subscribe(results => {
        this.faces = [];
        _.forEach(results, (value, index) => this.faces.push({ url: urls[index], faceId: value.persistedFaceId }));

        // 3. Call Detect on query face so we can establish a faceId 
        this.faceApi.detect(this.queryFace).subscribe(queryFaceDetectResult => {
          let queryFaceId = queryFaceDetectResult[0].faceId;

          // 4. Call Find Similar with the query face and the face list
          this.faceApi.findSimilar(faceListId, queryFaceId).subscribe(finalResults => {
            console.log('**findsimilar Results', finalResults);
            this.findSimilarResults = finalResults;
            this.loading = false;
          });
        });
      });
    });
  }

  getUrlForFace(faceId) {
    var face = _.find(this.faces, { faceId: faceId });
    return face.url;
  }

}
