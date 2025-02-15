import { urlJoin } from 'url-join-ts';

// https://github.com/netsentinel/vdb_web_client/blob/master/src/helpers/urlHelper.ts
export default class UrlHelper 
{
    public static getHostUrl = () =>
        window.location.protocol + '//' + window.location.host;

    public static getApiBaseUrl = () => urlJoin(
        this.getHostUrl(),
        "api");

    public static getMessageUrl = () => urlJoin(
        this.getApiBaseUrl(),
       "message");

    public static getMessageWsUrl = () => urlJoin(
        this.getMessageUrl(),
        "ws");
}