import xbmcaddon
import xbmcgui
import xbmc
import json

class Token(object):
    def __init__(self, j):
        self.__dict__ = json.loads(j)

tokenJsonFile = open(xbmc.translatePath('special://profile/addon_data/script.bitport.viewer/bitportToken.json'), 'r');
tokenJson=tokenJsonFile.read();

token = Token(tokenJson);

addon       = xbmcaddon.Addon();
addonname   = addon.getAddonInfo('name');
 
line1 = "Hello World!";
line2 = "We can write anything we want here22";
line3 = token.access_token;
 
xbmcgui.Dialog().ok(addonname, line1, line2, line3);
