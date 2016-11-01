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
import mimetypes

xbmc.log("executing main script")


tokenPath = xbmc.translatePath('special://profile/addon_data/plugin.video.bitportViewer/bitportToken.json')

api = BitportAPI(tokenPath)


_url = sys.argv[0]
_handle = int(sys.argv[1])



def get_list_videoItem(video,stream):

    label = video.name
    if stream:
        label = label + " (stream)"
        mimeType="video/mp4"
    else:
        mimeType = mimetypes.guess_type(video.filename, strict=False)[0]
        
    list_item = xbmcgui.ListItem(label=label)
    list_item.setInfo('video', {'title':label })
    list_item.setProperty('IsPlayable', 'true')

    list_item.setProperty('mimetype', mimeType)

    url=api.getUrl(video.code,stream)

    is_folder = False
    return (url, list_item, is_folder)

def get_list_folderItem(folder):
    list_item = xbmcgui.ListItem(label=folder.name)        
    list_item.setInfo('video', {'title': folder.name})

    query = {
        "action":"list",
        "code":folder.code
        }
    url = _url + "?" + urllib.urlencode(query)
    is_folder = True
    return (url, list_item, is_folder)

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
            if item.type is BP_FileType.other:
                continue
            listing.append(get_list_videoItem(item,False))
            if item.converted:
                listing.append(get_list_videoItem(item,True))
        elif item.__class__ is BP_Folder:
            listing.append(get_list_folderItem(item))

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
            play_video(params['code'],params['name'],params['filename'],params['stream'] == "True")
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