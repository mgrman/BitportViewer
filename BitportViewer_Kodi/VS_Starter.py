import sys
from BitportAPI import *


tokenPath=sys.argv[1]

api=BitportAPI(tokenPath);
allFiles=api.getFolder(None)
for file in allFiles:
    print file.__class__ is BP_Folder
