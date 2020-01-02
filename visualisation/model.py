# model.py

import pandas as pd
import matplotlib as plt
import functools.reduce 

class Model:
    def __init__(self):
        self.fileName = None
        self.fileContent = ""
        self.maxDays = "0"
        self.plotContent = ""
        self.columns = ""
    
    def rGetAttr(self, obj, attr, *args):
        def _getattr(obj, attr):
            return getattr(obj, attr, *args)
        return functools.reduce(_getattr, [obj] + attr.split('.'))

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
            self.fileContent = pd.read_csv(fileName)
            self.addAverageColumns()
            self.maxDays = len(self.fileContent.index)
            self.columns = self.fileContent.columns.values.tolist()
        else:
            self.fileContent = ""
            self.fileName = ""
            self.plotContent = ""
    
    def addAverageColumns(self):
        #energy average
        energyDf = self.fileContent.filter(regex="Energy$", axis=1)
        averageColumn = energyDf.mean(axis=1)
        self.fileContent["Average Energy"] = averageColumn

    def addFilter(self, parameter, include=True, value='', num=0, greater=0):
        #gets datatype of column
        dataType = self.rGetAttr(self.plotContent, parameter + '.dtype')
        if dataType == 'object':
            # filter strings
            if include:
                self.plotContent = self.plotContent[getattr(self.plotContent, parameter) == value]
            else:
                self.plotContent = self.plotContent[getattr(self.plotContent, parameter) != value]
        else:
            # filter numeric data
            if greater == 0:
                self.plotContent = self.plotContent[getattr(self.plotContent, parameter) > num]
            elif greater == 1:
                self.plotContent = self.plotContent[getattr(self.plotContent, parameter) == num]
            else:
                self.plotContent = self.plotContent[getattr(self.plotContent, parameter) < num]


    def getFileName(self):
        return self.fileName
        
    def getFileContents(self):
        return self.fileContent

    def getPlotContents(self):
        return self.plotContent

    def getColumns(self):
        return self.columns