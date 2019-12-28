# app.py

from PyQt5 import QtCore, QtGui, QtWidgets
from PyQt5.QtCore import QObject, pyqtSlot
from gui import Ui_MainWindow
import sys
from model import Model

class MainWindowUiClass(Ui_MainWindow):
    def __init__(self):
        super().__init__()
        self.model = Model()

    def setupUi(self, mw):
        super().setupUi(mw)

    def filterPrint(self, msg):
        self.filterBrowser.append(msg)

    def refreshAll(self):
        self.addLogButton.setEnabled(True)
        self.addTraceButton.setEnabled(True)
        self.exportAllButton.setEnabled(True)
        self.gifButton.setEnabled(True)
        self.daySpinBox.setEnabled(True)
        self.plotTypeComboBox.setEnabled(True)
        self.colourComboBox.setEnabled(True)
        self.agentAverageRadio.setEnabled(True)
        self.timeAverageRadio.setEnabled(True)
        self.xComboBox.setEnabled(True)
        self.yComboBox.setEnabled(True)
        self.horizontalSlider.setEnabled(True)

        self.lineEdit.setText(self.model.getFileName())

    #slots
    def returnPressedSlot(self):
        #self.debugPrint("enter pressed in line edit")
        fileName = self.lineEdit.text()
        if self.model.setFileName(fileName):
            self.model.setFileName(fileName)
            self.refreshAll()
        else:
            m = QtWidgets.QMessageBox()
            m.setText("Invalid file name!\n" + fileName)
            m.setIcon(QtWidgets.QMessageBox.Warning)
            m.setStandardButtons(QtWidgets.QMessageBox.Ok | QtWidgets.QMessageBox.Cancel)
            m.setDefaultButton(QtWidgets.QMessageBox.Cancel)
            ret = m.exec_()
            self.lineEdit.setText("")
            self.refreshAll()
            self.debugPrint("Invalid file specified: " + fileName)

    def browseSlot(self):
        #self.debugPrint("browse button pressed")
        options = QtWidgets.QFileDialog.Options()
        options |= QtWidgets.QFileDialog.DontUseNativeDialog
        fileName, _ = QtWidgets.QFileDialog.getOpenFileName(
                        None,
                        "QFileDialog.getOpenFileName()",
                        "",
                        "CSV Files (*.csv);;All Files (*)",
                        options=options)
        if fileName:
            self.model.setFileName(fileName)
            self.refreshAll()

    def noneAverageSelected(self):
        # can do push button setEnabled(True/False)
        pass

    def agentAverageSelected(self):
        pass

    def timeAverageSelected(self):
        pass

    def addLogSlot(self):
        pass

    def addAgentSlot(self):
        pass

    def exportGifSlot(self):
        pass

    def exportAllSlot(self):
        pass

    def plotColourSlot(self):
        pass

    def plotTypeSlot(self):
        pass

    def xAxisSlot(self):
        pass

    def yAxisSlot(self):
        pass

    def addFilterSlot(self):
        # msg is the filter setting to print
        self.filterPrint(msg)
        pass

    def removeAllFiltersSlot(self):
        self.textBrowser.clear()
        pass

    def filterParameterSelectedSlot(self):
        pass

def main():
    app = QtWidgets.QApplication(sys.argv)
    MainWindow = QtWidgets.QMainWindow()
    ui = MainWindowUiClass()
    ui.setupUi(MainWindow)
    MainWindow.show()
    sys.exit(app.exec_())

main()
