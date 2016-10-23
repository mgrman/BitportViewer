import sys
import json
import requests
import re

class BitportAPI:
    
    access_token = ""
    apiBaseUrl = "https://api.bitport.io/v2"

    isTvShowRegex = re.compile("[Ss]\d{1,2}[Ee]\d{1,2}")
    getNameRegex = re.compile("(^[a-zA-Z. _\-0-9()'\"]*?)\(?2\d\d\d|(^[a-zA-Z. _\-0-9()'\"]*)([Ss]\d{1,2}[Ee]\d{1,2})")

    def __init__(self,tokenPath):
        tokenJsonFile = open(tokenPath, 'r')
        tokenJson = tokenJsonFile.read()
        self.access_token = json.loads(tokenJson)["access_token"]


        
    def getFolder_raw(self,code):
        getFolderUrl = self.apiBaseUrl + "/cloud"

        if code != None :
            getFolderUrl = getFolderUrl + "/" + code
        
        resp = requests.get(getFolderUrl,headers={'Authorization':'Bearer ' + self.access_token})

        data = resp.json()

        files = data["data"][0]["files"]
        folders = data["data"][0]["folders"]

        #for folder in folders:
        #    subFiles = self.getFiles(folder["code"])
        #    files.extend(subFiles)

        result=BP_RawResult();
        result.folders=folders;
        result.files=files;
        return result

    def getUrl(self,code,converted):
        if converted:
            operation = "stream"
        else:
            operation = "download"
        getFileUrlUrl = self.apiBaseUrl + "/files/" + code + "/" + operation

        return getFileUrlUrl+"|Authorization=Bearer "+self.access_token;

        #resp = requests.get(getFileUrlUrl,headers={
        #    'Authorization':'Bearer ' + self.access_token,
        #    "Range": "bytes=0-0"})

        #return resp.url

    def convertFile(self, file):
        
        resultFile = BP_File()
        resultFile.code = file["code"]
        resultFile.filename = file["name"]

        nameMatch = self.getNameRegex.match(resultFile.filename)

        if nameMatch:
            movieGroup = nameMatch.group(1)
            tvShowGroup = nameMatch.group(2)
            tvShowEpisodeGroup = nameMatch.group(3)
            if movieGroup is not None:
                resultFile.name = movieGroup.replace("."," ").strip().title()
            elif tvShowGroup is not None:
                resultFile.name = tvShowGroup.replace("."," ").strip().title() + " " + tvShowEpisodeGroup.upper()
            else:
                resultFile.name = resultFile.filename
        if file["video"]:
            if self.isTvShowRegex.match(resultFile.name) is not None:
                resultFile.type = BP_FileType.tv_show
            else:
                resultFile.type = BP_FileType.movie
        else:
            resultFile.type = BP_FileType.other
        return resultFile;
    
    def convertFolder(self, folder):
        
        resultFile = BP_Folder()
        resultFile.code = folder["code"]
        resultFile.name = folder["name"]
        return resultFile;

    def getFolder(self,code):
        rawResult = self.getFolder_raw(code)

        
        result=[]
        for file in rawResult.files:
            resultFile = self.convertFile(file)
            result.append(resultFile)
        for folder in rawResult.folders:
            resultFolder = self.convertFolder(folder)
            result.append(resultFolder)
        return result


class BP_RawResult:
    folders = []
    files = []


class BP_FileType:
    movie = 1
    tv_show = 2
    other = 3

class BP_File:
    type = BP_FileType.other
    code = ""
    name = ""
    filename = ""
    
class BP_Folder:
    code = ""
    name = ""
