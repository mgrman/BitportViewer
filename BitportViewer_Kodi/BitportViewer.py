from urlparse import parse_qsl
import sys
import xbmc
import xbmcgui
import xbmcplugin
import json
import math
import urlparse
import urllib
from BitportAPI import *

xbmc.log("executing main script")


tokenPath = xbmc.translatePath('special://profile/addon_data/plugin.video.bitportViewer/bitportToken.json')

api = BitportAPI(tokenPath)


_url = sys.argv[0]
_handle = int(sys.argv[1])


def list_videos(code):
    """
    Create the list of playable videos in the Kodi interface.
    """
    # Get the list of videos
    items = api.getFolder(code)
    # Create a list for our items.
    listing = []
    # Iterate through videos.
    for item in items:
        if item.__class__ is BP_File:
            video = item

            if video.type is BP_FileType.other:
                continue

            # Create a list item with a text label and a thumbnail image.
            list_item = xbmcgui.ListItem(label=video.name)
            # Set additional info for the list item.
            list_item.setInfo('video', {'title': video.name })
            # Set graphics (thumbnail, fanart, banner, poster, landscape etc.)
            # for
            # the list item.
            # Here we use the same image for all items for simplicity's sake.
            # In a real-life plugin you need to set each image accordingly.
            #list_item.setArt({'thumb': video['thumb'], 'icon': video['thumb'],
            #'fanart': video['thumb']})
            # Set 'IsPlayable' property to 'true'.
            # This is mandatory for playable items!
            list_item.setProperty('IsPlayable', 'true')
            # Create a URL for the plugin recursive callback.
            # Example:
            # plugin://plugin.video.example/?action=play&video=http://www.vidsplay.com/vids/crab.mp4

            query = {
                "action":"play","code":video.code,"name":video.name,"filename":video.filename
                }
            url = _url + "?" + urllib.urlencode(query)
            # Add the list item to a virtual Kodi folder.
            # is_folder = False means that this item won't open any sub-list.
            is_folder = False
            # Add our item to the listing as a 3-element tuple.
            listing.append((url, list_item, is_folder))
        elif item.__class__ is BP_Folder:
            folder = item
            list_item = xbmcgui.ListItem(label=folder.name)        
            list_item.setInfo('video', {'title': folder.name})

            query = {
                "action":"list",
                "code":folder.code
                }
            url = _url + "?" + urllib.urlencode(query)
            is_folder = True
            listing.append((url, list_item, is_folder))

    # Add our listing to Kodi.
    # Large lists and/or slower systems benefit from adding all items at once
    # via addDirectoryItems
    # instead of adding one by ove via addDirectoryItem.
    xbmcplugin.addDirectoryItems(_handle, listing, len(listing))
    # Add a sort method for the virtual folder items (alphabetically, ignore
    # articles)
    xbmcplugin.addSortMethod(_handle, xbmcplugin.SORT_METHOD_LABEL_IGNORE_THE)
    # Finish creating a virtual folder.
    xbmcplugin.endOfDirectory(_handle)


def play_video(code,name,filename):
    """
    Play a video by the provided path.
    :param path: str
    """

    url = api.getUrl(code,False)

    # Create a playable item with a path to play.
    play_item = xbmcgui.ListItem(path=url,label=name,label2=filename)
    # Pass the item to the Kodi player.
    xbmcplugin.setResolvedUrl(_handle, True, listitem=play_item)


def router(paramstring):
    """
    Router function that calls other functions
    depending on the provided paramstring
    :param paramstring:
    """
    # Parse a URL-encoded paramstring to the dictionary of
    # {<parameter>: <value>} elements
    params = dict(parse_qsl(paramstring))
    # Check the parameters passed to the plugin
    if params:
        if params['action'] == 'play':
            # Play a video from a provided URL.
            play_video(params['code'],params['name'],params['filename'])
        if params['action'] == 'list':
            # Play a video from a provided URL.
            list_videos(params['code'])
    else:
        # If the plugin is called from Kodi UI without any parameters,
        # display the list of videos
        list_videos(None)

if __name__ == '__main__':
    # Call the router function and pass the plugin call parameters to it.
    # We use string slicing to trim the leading '?' from the plugin call
    # paramstring
    router(sys.argv[2][1:])