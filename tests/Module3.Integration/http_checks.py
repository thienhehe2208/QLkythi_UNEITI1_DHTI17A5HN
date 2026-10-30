# Họ và tên/Mã sinh viên: CHƯA CUNG CẤP.
# Nội dung: kiểm thử HTTP với ứng dụng thật, Session, bộ lọc quyền và anti-forgery.
# Chỉ dùng Database fixture mới do Program.cs của bộ kiểm thử tạo.
import argparse
import html
import http.cookiejar
import json
import re
import urllib.error
import urllib.parse
import urllib.request

parser = argparse.ArgumentParser()
parser.add_argument('--base-url', default='http://127.0.0.1:5075')
parser.add_argument('--metadata', required=True)
args = parser.parse_args()
meta = json.load(open(args.metadata, encoding='utf-8'))
passed = []

def check(value, name):
    if not value:
        raise AssertionError(name)
    passed.append(name)
    print('PASS:', name, flush=True)

class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None

class Client:
    def __init__(self):
        self.jar = http.cookiejar.CookieJar()
        self.opener = urllib.request.build_opener(urllib.request.ProxyHandler({}), urllib.request.HTTPCookieProcessor(self.jar), NoRedirect())

    def request(self, path, data=None):
        request = urllib.request.Request(args.base_url + path, data=None if data is None else urllib.parse.urlencode(data).encode())
        try:
            response = self.opener.open(request, timeout=30)
        except urllib.error.HTTPError as error:
            response = error
        return response.code, dict(response.headers), html.unescape(response.read().decode('utf-8'))

    def login(self, name):
        code, _, body = self.request('/Auth/DangNhap')
        check(code == 200, 'Trang đăng nhập mở được: ' + name)
        code, _, _ = self.request('/Auth/DangNhap', {'TenDangNhap': name, 'MatKhau': 'Module3@Test123', '__RequestVerificationToken': token(body)})
        check(code == 302, 'Đăng nhập thực qua AuthController: ' + name)

def token(body):
    match = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', body)
    if not match:
        raise AssertionError('Không tìm thấy anti-forgery token')
    return match.group(1)

anon = Client()
for path in ['/ThiSinh/HoSoCaNhan', '/ThiSinh/CapNhatHoSo', '/ThiSinh', '/HoSoThiSinh', '/HoSoThiSinh/Create', '/HoSoThiSinh/Details/1', '/HoSoThiSinh/Huy/1']:
    code, headers, _ = anon.request(path)
    check(code == 302 and '/Auth/DangNhap' in headers.get('Location', ''), 'Chưa đăng nhập bị chặn: ' + path)

a, b, admin, staff, fresh = Client(), Client(), Client(), Client(), Client()
for client, name in [(a, 'm3_a'), (b, 'm3_b'), (admin, 'm3_admin'), (staff, 'm3_staff'), (fresh, 'm3_fresh')]:
    client.login(name)

for client in [a, b]:
    code, headers, _ = client.request('/ThiSinh')
    check(code == 302 and 'TuChoiTruyCap' in headers.get('Location', ''), 'Thí sinh bị chặn URL quản trị')
for client in [admin, staff]:
    for path in ['/ThiSinh', '/LichThi', '/KetQuaThi']:
        check(client.request(path)[0] == 200, 'Trang quản trị/hồi quy Module 5: ' + path)
    code, headers, _ = client.request('/HoSoThiSinh/Create')
    check(code == 302 and 'TuChoiTruyCap' in headers.get('Location', ''), 'Admin/cán bộ không nộp thay thí sinh')

body = a.request('/ThiSinh/HoSoCaNhan')[2]
candidate_a = int(re.search(r'TS-(\d+)', body).group(1))
check('Hồ sơ đã nộp' in body and 'Nộp hồ sơ' in body and 'Lịch thi' in body, 'Menu Module 3 và Module 5 cùng tồn tại')
check(a.request('/ThiSinh/CapNhatHoSo')[0] == 200, 'Form cập nhật Razor mở được')
check('Bạn chưa khai báo hồ sơ cá nhân' in fresh.request('/ThiSinh/HoSoCaNhan')[2], 'Tài khoản mới có trạng thái rỗng')
check('Bạn chưa khai báo hồ sơ cá nhân' in fresh.request('/ThiSinh/HoSoCaNhan')[2], 'GET lặp không tạo hồ sơ')

form_body = a.request('/ThiSinh/CapNhatHoSo')[2]
data = {'__RequestVerificationToken': token(form_body), 'HoTen': 'A cập nhật qua HTTP', 'NgaySinh': '2002-04-15', 'GioiTinh': 'Nu', 'SoDienThoai': '0912345678', 'Email': 'a@example.test', 'SoLanThiTruocDay': '0', 'MaTaiKhoan': meta['b'], 'MaThiSinh': '999999', 'TrangThai': 'NgungHoatDong'}
check(a.request('/ThiSinh/CapNhatHoSo', data)[0] == 302, 'POST cá nhân hợp lệ bỏ qua trường giả mạo')
body = a.request('/ThiSinh/HoSoCaNhan')[2]
check('A cập nhật qua HTTP' in body and 'Ngừng hoạt động' not in body, 'Cá nhân đúng A và không đổi trạng thái giả mạo')
check('A cập nhật qua HTTP' not in b.request('/ThiSinh/HoSoCaNhan')[2], 'Không cập nhật nhầm hồ sơ B')
missing_count = dict(data)
missing_count.pop('SoLanThiTruocDay')
code, _, validation_body = a.request('/ThiSinh/CapNhatHoSo', missing_count)
check(code == 200 and 'Vui lòng nhập số lần thi trước đây' in validation_body, 'POST thiếu trường số lần không bị hiểu thành 0')
check(a.request('/ThiSinh/CapNhatHoSo', {'HoTen': 'no token'})[0] == 400, 'POST thiếu anti-forgery bị chặn')

admin_body = admin.request('/ThiSinh/Details/' + str(candidate_a))[2]
check('Hồ sơ đăng ký liên quan' in admin_body and 'Cập nhật trạng thái' in admin_body, 'Chi tiết quản trị Razor và nút Admin mở được')
staff_body = staff.request('/ThiSinh/Details/' + str(candidate_a))[2]
check('Cập nhật trạng thái' not in staff_body, 'Cán bộ chỉ tra cứu, không có quyền đổi trạng thái')
code, headers, _ = staff.request('/ThiSinh/CapNhatTrangThai?id=' + str(candidate_a), {'TrangThai': 'NgungHoatDong'})
check(code == 302 and 'TuChoiTruyCap' in headers.get('Location', ''), 'POST trực tiếp của cán bộ bị bộ lọc quyền chặn')

path = '/HoSoThiSinh/Create?maKyThi=' + str(meta['httpExam'])
code, _, body = a.request(path)
check(code == 200 and 'Đang nhận hồ sơ' in body, 'Chọn trước kỳ thi từ link Module 2')
bad = a.request('/HoSoThiSinh/Create?maKyThi=2147483647')[2]
check('Kỳ thi đã chọn không tồn tại' in bad, 'Kỳ thi sai không âm thầm chọn kỳ khác')
csrf = token(body)
post = {'__RequestVerificationToken': csrf, 'MaKyThi': meta['httpExam'], 'MucDichDuThi': 'Đăng ký qua kiểm thử HTTP', 'GhiChu': 'HTTP fixture', 'MaThiSinh': '999999', 'NgayNop': '2000-01-01', 'TrangThai': 'Dat', 'NgayXuLy': '2000-01-01', 'NhanXetXetDuyet': 'INJECT_REVIEW', 'MaNguoiXuLy': meta['admin']}
code, headers, _ = a.request('/HoSoThiSinh/Create', post)
check(code == 302, 'POST nộp hồ sơ qua toàn bộ MVC pipeline')
detail_path = headers['Location']
code, _, body = a.request(detail_path)
check(code == 200 and 'Chờ duyệt' in body and 'INJECT_REVIEW' not in body and '01/01/2000' not in body, 'POST bỏ qua trạng thái/ngày/xét duyệt giả mạo')
check('Chưa có kết quả được công bố' in body and 'Hồ sơ chưa có lịch thi' in body, 'Chi tiết xử lý lịch/kết quả chưa có')
check(b.request(detail_path)[0] == 404, 'Đổi ID URL không đọc được hồ sơ A')
new_id = int(re.search(r'/Details/(\d+)', detail_path).group(1))
check(b.request('/HoSoThiSinh/Huy/' + str(new_id))[0] == 404, 'GET xác nhận hủy của người khác bị chặn')
code, _, duplicate = a.request('/HoSoThiSinh/Create', post)
check(code == 200 and 'đã có hồ sơ đang xử lý' in duplicate and 'chonKyThi' in duplicate, 'POST trùng báo lỗi và giữ dropdown')
check('M3 ' in a.request('/HoSoThiSinh?trang=999999')[2], 'Phân trang ngoài biên vẫn hiện dữ liệu đúng chủ sở hữu')
code, _, confirm = a.request('/HoSoThiSinh/Huy/' + str(new_id))
check(code == 200 and 'Xác nhận hủy đăng ký' in confirm, 'GET xác nhận hủy mở được')
check('Chờ duyệt' in a.request(detail_path)[2], 'GET hủy không thay đổi dữ liệu')
check(a.request('/HoSoThiSinh/HuyXacNhan/' + str(new_id), {})[0] == 400, 'POST hủy thiếu token bị chặn')
check(a.request('/HoSoThiSinh/HuyXacNhan/' + str(new_id), {'__RequestVerificationToken': token(confirm)})[0] == 302, 'POST hủy hợp lệ')
check('Đã hủy' in a.request(detail_path)[2], 'Hồ sơ chuyển Đã hủy và vẫn xem được')

for path in ['/LichThi', '/KetQuaThi', '/Home', '/']:
    check(a.request(path)[0] == 200, 'Hồi quy sau tích hợp: ' + path)
body = a.request('/ThiSinh/HoSoCaNhan')[2]
check(a.request('/Auth/DangXuat', {'__RequestVerificationToken': token(body)})[0] == 302, 'Đăng xuất thật')
check(a.request('/HoSoThiSinh')[0] == 302, 'Session cũ không tiếp tục xem hồ sơ sau đăng xuất')
print('RESULT:', len(passed), 'HTTP assertions passed', flush=True)
