import os
import subprocess
import time
from pywinauto import Application
from pywinauto import Desktop

documents_path = os.path.join(os.environ['USERPROFILE'], 'Documents\\ppk')
ppk = os.path.join(documents_path, "svn-psync-a-rd2users-075.ppk")
ppk2 = os.path.join(documents_path, "svn-psync3-rd2-075.ppk")
key = r"LLb^9PWVe~"
proc = subprocess.Popen(f'"C:\\Program Files\\PuTTY\\pageant.exe" "{ppk}"')
#time.sleep(0.1)
dialog = Desktop(backend="uia").window(title_re="Pageant:.*")
edit_elements = dialog.descendants(control_type="Edit")
edit_elements[0].set_edit_text(key)
dialog[u'OK'].click()
#time.sleep(0.1)

key2 = r"X3>TNp:%X_"
proc = subprocess.Popen(f'"C:\\Program Files\\PuTTY\\pageant.exe" "{ppk2}"')
#time.sleep(0.1)
dialog = Desktop(backend="uia").window(title_re="Pageant:.*")
edit_elements = dialog.descendants(control_type="Edit")
edit_elements[0].set_edit_text(key2)
dialog[u'OK'].click()
