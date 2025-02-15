import React, { useState, useEffect } from "react";
import useWebSocket from "react-use-websocket";
import UrlHelper from "../helpers/urlHelper";
import { format } from 'date-fns'
import './Reader.css';
import Message from "../models/message";

const Reader: React.FC = () => {
  const [messages, setMessages] = useState<Message[]>([]);
  const [socketUrl, setSocketUrl] = useState<string | null>(null);
  const { lastMessage: socketMessage, readyState: socketState } = useWebSocket(socketUrl,{ shouldReconnect: () => true });

  useEffect(() => {
    const lastMessageId = (messages?.length ?? 0) > 0 ? messages[messages.length-1].id : 0; 
    setSocketUrl(UrlHelper.getMessageWsUrl() + `?lastIdOnClient=${lastMessageId}`);
  }, [socketState]);

  useEffect(() => {
    if (socketMessage?.data) {
        const newMessages: Message[] = JSON.parse(socketMessage.data);
        if(newMessages) {
          setMessages((prev) => [...prev, ...newMessages.sort((a,b) => a.id - b.id)]);
        }
    }
  }, [socketMessage]);

  return (
    <div style={{ flexDirection: "column", fontFamily:"monospace" }}>
      <table>
        <thead>
          <tr>
            <th>id</th>
            <th>seq</th>
            <th>time</th>
            <th>text</th>
          </tr>
        </thead>

        <tbody>
          {messages.map(msg => (
            <tr key={msg.id}>
              <td>{msg.id}</td>
              <td>{msg.seq}</td>
              <td>{format(msg.createdAt, "yyyy-MM-dd HH:mm:ss")}</td>
              <td>{msg.text}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default Reader;
