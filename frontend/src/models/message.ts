export default class Message {
  public id: number = 0;
  public seq: number = 0;
  public createdAt: Date = new Date();
  public text: string = "";
}
