import { useRef, useState } from "react";
import ApiHelper from "../helpers/apiHelper";
import Message from "../models/message";

const Writer: React.FC = () => {
  const seq = useRef(0);
  const inputRef = useRef<HTMLInputElement>(null);
  const [lastStatus, setLastStatus] = useState(0);

  const send = async () => {
    const val = inputRef.current?.value;

    if(!val || val.length < 1)
        return;

    const msg = new Message();
    msg.seq = ++seq.current;
    msg.text = val;

    setLastStatus(await ApiHelper.PushMessage(msg));
  }

  return (
    <span style={{ flexDirection: "column" }}>
        <input style={{maxWidth:"300px"}} type="text" maxLength={128} ref={inputRef} />
        <button onClick={send}>send</button>
        {
            lastStatus > 0 
            ?
            <span>
            status: {lastStatus}
            </span> 
            : 
            <span/>
        }
    </span>
  );
};

export default Writer;
