import { useEffect, useState } from "react";
import ApiHelper from "../helpers/apiHelper";
import Message from "../models/message";
import { format } from 'date-fns';

const History: React.FC = () => {
  const [forTheLastNMinutes, setForTheLastNMinutes] = useState(10);
  const [messages, setMessages] = useState<Message[]>([]);

  useEffect(() => {
    const from = new Date(new Date().setMinutes(new Date().getMinutes() - forTheLastNMinutes));
    const to = new Date();
    (async () => setMessages(await ApiHelper.GetMessages(from, to)))();
  }, [forTheLastNMinutes]);

  return (
    <span style={{ flexDirection: "column", fontFamily:"monospace" }}>
      <span style={{flexDirection:"row"}}>
        for the last <input type="number" step={1} value={forTheLastNMinutes} onChange={(e) => setForTheLastNMinutes(parseInt(e.target.value))} /> minutes
        </span>
        id - seq - timestamp - text
      {messages.map(msg => (
        <span key={msg.id}>
          {msg.id.toString()}&nbsp;&nbsp;
          {msg.seq.toString()}&nbsp;&nbsp;
          {format(msg.createdAt, "yyyy-MM-dd HH:mm:ss")}&nbsp;&nbsp;
          {msg.text}
        </span>
      ))}
    </span>
  );
};

export default History;
