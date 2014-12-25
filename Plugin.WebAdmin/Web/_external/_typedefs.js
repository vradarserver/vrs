// This is not source. It's jsdoc typedefs for anonymous objects.

/**
 * @typedef {{
 * pageId:          string,
 * jsonUrl:         string,
 * refreshPeriod:   number
 * }} VRS_WEBADMIN_VIEW_SETTINGS
 */
VRS_WEBADMIN_VIEW_SETTINGS;

/**
 * @typedef {{
 * IsRunning:       bool
 * }} VRS_WEBADMIN_VIEWDATA_BASEVIEW
 */
VRS_WEBADMIN_VIEWDATA_BASEVIEW;

/**
 * @extends VRS_WEBADMIN_VIEWDATA_BASEVIEW
 * @typedef {{
 * Caption:                         string,
 * ProductName:                     string,
 * Version:                         string,
 * Copyright:                       string,
 * Description:                     string,
 * IsMono:                          boolean
 * }} VRS_WEBADMIN_VIEWDATA_ABOUT
 */
VRS_WEBADMIN_VIEWDATA_ABOUT;

/**
 * @extends VRS_WEBADMIN_VIEWDATA_BASEVIEW
 * @typedef {{
 * InvalidPluginCount:              number,
 * LogFileName:                     string,
 * NewVersionAvailable:             boolean,
 * NewVersionDownloadUrl:           string,
 * RebroadcastServersConfiguration: string,
 * UPnpEnabled:                     boolean,
 * UPnpRouterPresent:               boolean,
 * UPnpPortForwardingActive:        boolean,
 * WebServerIsOnline:               boolean,
 * WebServerLocalAddress:           string,
 * WebServerNetworkAddress:         string,
 * WebServerExternalAddress:        string
 * }} VRS_WEBADMIN_VIEWDATA_MAIN
 */
VRS_WEBADMIN_VIEWDATA_MAIN;