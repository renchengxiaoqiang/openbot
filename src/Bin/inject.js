const script = document.createElement("script");
script.type = "text/javascript";
script.src = "https://iseiya.taobao.com/imsupport";
document.getElementsByTagName("body")[0].appendChild(script);

if (typeof window.___setupWebSocket === 'undefined') {
  window._buyerCache = new Map()
  window.___setupWebSocket = function() {
    if (window.chatWebsocket && window.chatWebsocket.readyState === WebSocket.OPEN) {
      return; // 如果已有活跃连接，不再创建新连接
    }

    let heartbeatInterval;
    let socket = new WebSocket("ws://127.0.0.1:41010");

    socket.onopen = async (e) => {
      console.log('[WebSocket] Connected');
      startHeartbeat();
      window.chatWebsocket = socket; // 存储到全局变量
    };

    socket.onmessage = async function(event) {
      let param = JSON.parse(event.data);
      if (param.method === 'execute') {
        try {
          const res = await eval(param.expression);
          socket.send(JSON.stringify({ type: 'execute', response: JSON.stringify(res) }));
        } catch (err) {
          console.error('Eval error:', err);
        }
      }
    };

    socket.onclose = function(event) {
      console.log('[WebSocket] Connection closed, reconnecting...');
      clearInterval(heartbeatInterval);
      if (window.chatWebsocket === socket) {
        window.chatWebsocket = null; // 清理引用
      }
      setTimeout(window.___setupWebSocket, 3000);
    };

    function startHeartbeat() {
      heartbeatInterval = setInterval(() => {
        if (socket.readyState === WebSocket.OPEN) {
          socket.send(JSON.stringify({ type: 'hi' }));
        } else {
          clearInterval(heartbeatInterval); // 如果连接断开，停止心跳
        }
      }, 3000);
    }

    socket.onerror = function(error) {
      console.error('[WebSocket] Error:', error);
    };
  };

  window.___setupWebSocket(); // 初始化



  if(typeof(window.___qnww)=='undefined'){ 
    window.___qnww = window.onEventNotify;
    window.onEventNotify = function (sid, name, a, data){
        window.___qnww(sid, name, a, data);
        name = JSON.parse(name);
        if(sid.indexOf('onConversationChange')>=0){
          updateFromConversation(name);
          window.chatWebsocket.send(JSON.stringify({type:'onConversationChange',response: JSON.stringify({loginID:window._vs.loginID,conversation:name})}))
          console.log('onConversationChange,'+name.nick);     
        }else if(sid.indexOf('onConversationAdd')>=0){          
          updateFromConversation(name);
          window.chatWebsocket.send(JSON.stringify({type:'onConversationAdd',response: JSON.stringify({loginID:window._vs.loginID,conversation:name})}))
        }else if(sid.indexOf('onConversationClose')>=0){          
          updateFromConversation(name);
          window.chatWebsocket.send(JSON.stringify({type:'onConversationClose',response: JSON.stringify({loginID:window._vs.loginID,conversation:name})}))
        }
        else if(sid.indexOf('OnChatDlgActive')>=0){
          window.chatWebsocket.send(JSON.stringify({type:'onChatDlgActive',response:JSON.stringify({loginID:window._vs.loginID,conversation:window._vs.conversationID})}))
        }
    }
}

  //千牛通知，要先配置子账号接受通知
  QN.regEvent('bench.msgcenter.newmsgnotify',res=>{
    window.chatWebsocket.send(
      JSON.stringify(
      {
        type: 'messageCenterNotify',
        response: res
      })
    )
  })


  if (typeof(window.onInvokeNotifyDelegate) == 'undefined') {
    imsdk.on(['im.singlemsg.onReceiveNewMsg'], cids => {
      cids.forEach(async cid=>{
        let conv = getCacheConv(cid.ccode)
        if(conv == undefined){
          conv = await getRemoteMsg(cid.ccode)
        }
        window.chatWebsocket.send(JSON.stringify({
          type:'onShopRobotReceriveNewMsgs',
          response:JSON.stringify({loginID:window._vs.loginID,conversation:conv})
        })); 
        console.log('onShopRobotReceriveNewMsgs,'+JSON.stringify(conv));
        // if(cid.ccode !== window._conversationId.ccode)
        // {
        //     imsdk.invoke('im.singlemsg.GetNewMsg', {
        //         ccode: cid.ccode
        //     }).then(response => {
        //         window.chatWebsocket.send(JSON.stringify({type:'receiveNewMsg',response:JSON.stringify(response)}));                       
        //     });
        // }
      })
    })
    window.onInvokeNotifyDelegate = window.onInvokeNotify;
    window.onInvokeNotify = function(sid, status, response) {
      window.onInvokeNotifyDelegate(sid, status, response);
      
        var task = TASK_CACHE[sid];
        if (task.config.fn == 'im.singlemsg.GetNewMsg' && task.config.param.ccode == window._conversationId.ccode) {
            window.chatWebsocket.send(JSON.stringify({type:'receiveNewMsg',response}));
        } 
    }
  }
}

async function getRemoteMsg(ccode){
  var remoteMsg = await imsdk.invoke('im.singlemsg.GetRemoteHisMsg', {
                    cid:
                    {
                        ccode,
                        type:1
                    },
                    count: 3,
                    gohistory: 1,
                    msgid:'-1',
                    msgtime: '-1',
                  })
  var buyer = { ccode }
  var msgs = remoteMsg.result.msgs;
  for(var idx = 0; idx < msgs.length; idx++){
    if(msgs[idx].loginid.nick != msgs[idx].fromid.nick){
      buyer = msgs[idx].fromid
      break
    }
  }
  return buyer
}

function getCacheConv(ccode) {
  try {
      if (!window._buyerCache.has(ccode))
        updateBuyerCacheFromLocal();
      return _buyerCache.get(ccode);
  } catch (e) {
      console.error("get conversation error", e.message)
  }
}

function updateFromConversation(conv) {
  try {
      if (window._buyerCache.has(conv.ccode))
          return;
      _buyerCache.set(conv.ccode, conv);
  } catch (e) {
      console.error("update from conversation error", e.message)
  }
}

function updateBuyerCacheFromLocal() {
  try {
      const { _db: { msgDataMap } } = window;
      Array.from(msgDataMap).forEach(([ccode, messages]) => {
          if (window._buyerCache.has(ccode)) return;          
          for (const message of messages) {
              const { ext: { receiver_nick: receiverNick, sender_nick: senderNick } = {}, originBanamaMessage: {toid, fromid} } = message;
              if (!senderNick || !receiverNick) continue;
              
              if (senderNick.includes(window._vs.loginID.nick)) {
                  _buyerCache.set(ccode, toid);
              }
              
              if (receiverNick.includes(window._vs.loginID.nick)) {
                  _buyerCache.set(ccode, fromid);
              }
              
              break; // Only process the first valid message
          }
      });
  } catch (error) {
      console.error("Failed to update cache:", error.message);
  }
}