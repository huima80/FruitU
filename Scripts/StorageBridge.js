(function () {
    function handleRequest(event) {
        try {
            var data = JSON.parse(event.data);
            var storage = data.ss != "1" ? window.localStorage : window.sessionStorage;
            if (data.op === 'M') { //getAll Message
                var arrJs = [];
                for (var key in localStorage) {
                    if (/^_m_/.test(key)) {
                        arrJs.push({
                            key: key,
                            obj: localStorage.getItem(key)
                        });
                    }
                }

                event.source.postMessage(JSON.stringify({
                    id: data.id,
                    key: data.key,
                    value: arrJs,
                    timer: data.timer
                }), event.origin);
            } else if (data.op === 'W') { //写操作  
                storage.setItem(data.key, JSON.stringify(data.value));
                event.source.postMessage(event.data, event.origin);
            } else if (data.op === 'D') { //删除  
                storage.removeItem(data.key);
                event.source.postMessage(event.data, event.origin);
            } else if (data.op === 'X') { //清空  
                storage.clear();
                event.source.postMessage(event.data, event.origin);
            } else { //默认：读操作  
                var value = JSON.parse(storage.getItem(data.key));
                event.source.postMessage(JSON.stringify({
                    id: data.id,
                    key: data.key,
                    value: value,
                    timer: data.timer
                }), event.origin);
            }
        } catch (e) {
            event.source.postMessage(event.data, event.origin);
        }
    }


    window.addEventListener("message", handleRequest, false);

})();