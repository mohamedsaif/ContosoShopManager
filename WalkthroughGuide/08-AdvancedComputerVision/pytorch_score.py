# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license.
# Adapted from https://github.com/Azure/MachineLearningNotebooks/tree/master/how-to-use-azureml/training-with-deep-learning/train-hyperparameter-tune-deploy-with-tensorflow

import torch
import torch.nn as nn
from torchvision import transforms
import json

from azureml.core.model import Model


def init():
    global model
    model_path = Model.get_model_path('pytorch-shelf')
    model = torch.load(model_path, map_location=lambda storage, loc: storage)
    model.eval()


def run(input_data):
    input_data = torch.tensor(json.loads(input_data)['data'])

    # get prediction
    with torch.no_grad():
        output = model(input_data)
        classes = ['Compliant', 'Non-Compliant']
        softmax = nn.Softmax(dim=1)
        pred_probs = softmax(output).numpy()[0]
        index = torch.argmax(output, 1)

    result = {"label": classes[index], "probability": str(pred_probs[index])}
    return result
