import os
import re
import io
import html
import http.server
import json 
from urllib.parse import unquote

def natural_key(s):
    return [int(text) if text.isdigit() else text.lower() for text in re.split(r'(\d+)', s)]

# Show pictures as a browsable album
class CustomHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    def list_directory(self, path):
        try:
            file_list = os.listdir(path)
        except OSError:
            self.send_error(404, "No permission to list directory")
            return None

        image_extensions = ('.png', '.jpg', '.jpeg', '.gif')
        images = [name for name in file_list if name.lower().endswith(image_extensions)]
        images.sort(key=natural_key)

        displaypath = html.escape(unquote(self.path))
        response = []
        response.append('<!DOCTYPE html>')
        response.append('<html>')
        response.append('<head>')
        response.append('<meta charset="utf-8">')
        response.append(f'<title>{displaypath}</title>')
        response.append('<style>')
        response.append('  body { margin: 0; background-color: #121212; overflow: hidden; }')
        response.append('  #viewer { position: relative; width: 100vw; height: 100vh; }')
        response.append('  #fullscreenImage { width: 100%; height: 100%; object-fit: contain; cursor: pointer; display: block; }')
        response.append('  #pageCounter { position: absolute; bottom: 20px; width: 100%; text-align: center; color: #e0e0e0; font-family: Arial, sans-serif; font-size: 20px; }')
        response.append('</style>')
        response.append('</head>')
        response.append('<body>')
        response.append('<div id="viewer">')
        response.append('<img id="fullscreenImage" src="" alt="Image Viewer">')
        response.append('<div id="pageCounter"></div>')
        response.append('</div>')
        response.append('<script>')
        response.append('var images = ' + json.dumps(images) + ';')
        response.append('var currentIndex = 0;')
        response.append('var imgElement = document.getElementById("fullscreenImage");')
        response.append('var pageCounter = document.getElementById("pageCounter");')
        response.append('function updateURL(index) {')
        response.append('  let url = new URL(window.location);')
        response.append('  url.searchParams.set("page", index);')
        response.append('  window.history.replaceState({}, "", url);')
        response.append('}')
        response.append('function showImage(index) {')
        response.append('  if (index >= 0 && index < images.length) {')
        response.append('    imgElement.src = images[index];')
        response.append('    pageCounter.innerText = "page " + (index + 1) + "/" + images.length;')
        response.append('    updateURL(index);')
        response.append('  }')
        response.append('}')
        response.append('// On load, read "page" from URL if it exists')
        response.append('const urlParams = new URLSearchParams(window.location.search);')
        response.append('var pageParam = parseInt(urlParams.get("page"));')
        response.append('if (!isNaN(pageParam) && pageParam >= 0 && pageParam < images.length) {')
        response.append('  currentIndex = pageParam;')
        response.append('}')
        response.append('// Mouse click navigation')
        response.append('imgElement.addEventListener("click", function(event) {')
        response.append('  if (event.clientX < window.innerWidth / 2) {')
        response.append('    currentIndex = (currentIndex - 1 + images.length) % images.length;')
        response.append('  } else {')
        response.append('    currentIndex = (currentIndex + 1) % images.length;')
        response.append('  }')
        response.append('  showImage(currentIndex);')
        response.append('});')
        response.append('// Keyboard navigation using left and right arrow keys')
        response.append('document.addEventListener("keydown", function(event) {')
        response.append('  if (event.key === "ArrowLeft") {')
        response.append('    currentIndex = (currentIndex - 1 + images.length) % images.length;')
        response.append('    showImage(currentIndex);')
        response.append('  } else if (event.key === "ArrowRight") {')
        response.append('    currentIndex = (currentIndex + 1) % images.length;')
        response.append('    showImage(currentIndex);')
        response.append('  }')
        response.append('});')
        response.append('showImage(currentIndex);')
        response.append('</script>')
        response.append('</body>')
        response.append('</html>')

        encoded = "\n".join(response).encode("utf-8", "surrogateescape")
        self.send_response(200)
        self.send_header("Content-type", "text/html; charset=utf-8")
        self.send_header("Content-Length", str(len(encoded)))
        self.end_headers()
        return io.BytesIO(encoded)

if __name__ == "__main__":
    import socketserver
    PORT = 8000
    with socketserver.TCPServer(("", PORT), CustomHTTPRequestHandler) as httpd:
        print(f"Serving at port {PORT}")
        httpd.serve_forever()
