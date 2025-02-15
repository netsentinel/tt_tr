import axios from 'axios';
import UrlHelper from './urlHelper';
import Message from '../models/message';

export default class ApiHelper 
{
    public static PushMessage = async (msg: Message) => {
        console.log(msg);
        return (await axios.post(UrlHelper.getMessageUrl(), msg)).status;
    }

    public static GetMessages = async(from: Date, to:Date) => {
        console.log(from, to);
        return (await axios.get<Message[]>(UrlHelper.getMessageUrl() + `?from=${from.toISOString()}&to=${to.toISOString()}`)).data;
    }
} 