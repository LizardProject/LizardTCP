##ACHTUNG / WARNING
Right now this project will write with too bad quality code, because i need to write something base and make from crap beautiful and clear code


![alt tag](https://github.com/GiaNTizmO/LizardTCP/blob/master/bg.png)
# LizardTCP
Fast, async, simple and scalable TCP/UDP/HTTP/HTTPS/WS/WSS proxy server with some interactive protection and statistics, written in C#.

## Example rule configuration

```json
[
  {
    "ruleName": "ExampleForward",
    "ruleType": "HTTP",
    "ruleIP": "1.1.1.1",
    "rulePort": 80,
    "bindIP": "127.0.0.1",
    "bindPort": 8081
  }
]
```

## Example proxy configuration

```json
{
  "proxy_mode": "transparent",
  "debug": true,
  "useCFHeaders": false,
  "bindingIP": "127.0.0.1",
  "bindingPort": "8080"
}
```

Development stages:
- [x] Core
- [ ] Web Configurator
- [ ] Guard (flood check, rate-limit, etc)
- [x] Transparent and proxy forwarding
- [ ] HTTPS/WSS Support
- [ ] WebSocket Support
- [x] TCP Support
- [ ] UDP Support
