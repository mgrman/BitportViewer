import sys
from BitportAPI import *


tokenPath=sys.argv[1]

api=BitportAPI(tokenPath);
allFiles=api.getFolder(None)
for file in allFiles:
    print api.getUrl(file.code,file.converted,file.filename)
