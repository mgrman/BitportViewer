import sys
from BitportAPI import *


tokenPath=sys.argv[1]

api=BitportAPI(tokenPath);
allFiles=api.getAllFiles()
for file in allFiles:
    if file.type== BP_FileType.movie:
        print file.name