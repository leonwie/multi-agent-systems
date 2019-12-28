# model.py

import pandas as pd
import matplotlib as plt

class Model:
    def __init__(self):
        self.fileName = None
        self.fileContent = ""
        self.maxDays = "0"
        
    def isValid(self, fileName):
        if fileName.endswith('.csv'):
            try:
                file = open(fileName, 'r')
                file.close()
                return True
            except:
                return False
        else:
            return False

    def setFileName(self, fileName):
        if self.isValid(fileName):
            self.fileName = fileName
            self.fileContents = pd.read_csv(fileName)
            self.maxDays = len(self.fileContents.index)
        else:
            self.fileContents = ""
            self.fileName = ""
            
    def getFileName(self):
        return self.fileName
        
    def getFileContents(self):
        return self.fileContents
