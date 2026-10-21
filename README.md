plan to make share product, share shop generic, not just recipe specific
muta LoggerService in prorpiul controller
translate into other languages
comaparare cu mobile screen cu screen
add product to a shopping list de pe shop list edit?????
This won't eliminate disconnects caused by an actual server restart/deploy or an IIS app-pool recycle — that's infra, not app code. Since your other project notes mention these services run under IIS, it's worth separately confirming the site has the WebSockets Windows feature enabled and that nothing in front of it (ARR/reverse proxy) has an idle timeout shorter than the circuit's keep-alive — that's the other common cause of this overlay showing up a lot.