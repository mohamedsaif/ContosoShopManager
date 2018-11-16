import { TestBed, inject } from '@angular/core/testing';

import { FaceApiServiceService } from './face-api-service.service';

describe('FaceApiServiceService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [FaceApiServiceService]
    });
  });

  it('should be created', inject([FaceApiServiceService], (service: FaceApiServiceService) => {
    expect(service).toBeTruthy();
  }));
});
