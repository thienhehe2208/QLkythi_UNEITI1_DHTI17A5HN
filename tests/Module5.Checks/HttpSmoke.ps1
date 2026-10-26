# Phạm Quang Minh — 23103100201: Kiểm thử HTTP bằng tài khoản demo, không dùng database nhóm.
param([string]$BaseUrl = 'http://127.0.0.1:5075')
$ErrorActionPreference = 'Stop'
$script:Passed = 0
function Assert-Check($Condition, [string]$Title) {
    if (-not $Condition) { throw "FAIL: $Title" }
    $script:Passed++
    Write-Host "PASS: $Title"
}
function Token($Html) {
    $match = [regex]::Match($Html, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"')
    if (-not $match.Success) { throw 'Không tìm thấy antiforgery token' }
    return [System.Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
}
function Login([string]$Name) {
    $session = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
    $login = Invoke-WebRequest "$BaseUrl/Auth/DangNhap" -WebSession $session
    $response = Invoke-WebRequest "$BaseUrl/Auth/DangNhap" -Method Post -WebSession $session -Body @{
        TenDangNhap = $Name; MatKhau = 'Module5@123'; __RequestVerificationToken = (Token $login.Content)
    }
    Assert-Check ($response.StatusCode -eq 200 -and $response.Content.Contains('Demo ' + $Name)) "Đăng nhập $Name"
    return $session
}
$anon = Invoke-WebRequest "$BaseUrl/LichThi" -MaximumRedirection 0 -SkipHttpErrorCheck -ErrorAction SilentlyContinue
Assert-Check ($anon.StatusCode -eq 302 -and $anon.Headers.Location -match '/Auth/DangNhap') 'Khách chưa đăng nhập bị chuyển đến đăng nhập'
$staff = Login 'm5_canbo'
foreach ($path in @('/LichThi', '/LichThi/Create', '/LichThi/Details/1', '/LichThi/Edit/1', '/KetQuaThi', '/KetQuaThi/Create', '/KetQuaThi/Details/1', '/KetQuaThi/Edit/1')) {
    $page = Invoke-WebRequest "$BaseUrl$path" -WebSession $staff
    Assert-Check ($page.StatusCode -eq 200 -and $page.Content -notmatch 'An unhandled exception') "Render $path"
}
$badCsrf = Invoke-WebRequest "$BaseUrl/LichThi/Huy/1" -Method Post -WebSession $staff -SkipHttpErrorCheck
Assert-Check ($badCsrf.StatusCode -eq 400) 'POST không có antiforgery token bị chặn'
$student = Login 'm5_thisinh01'
foreach ($path in @('/LichThi/Create', '/KetQuaThi/Create', '/LichThi/Edit/1', '/KetQuaThi/Edit/1')) {
    $denied = Invoke-WebRequest "$BaseUrl$path" -WebSession $student -SkipHttpErrorCheck
    Assert-Check ($denied.StatusCode -eq 403) "Thí sinh không được truy cập $path"
}
foreach ($path in @('/LichThi/Details/1', '/KetQuaThi/Details/1')) {
    $denied = Invoke-WebRequest "$BaseUrl$path" -WebSession $student -SkipHttpErrorCheck
    Assert-Check ($denied.StatusCode -eq 404) "Không lộ dữ liệu người khác qua $path"
}
$studentPage = Invoke-WebRequest "$BaseUrl/LichThi" -WebSession $student
$deniedPost = Invoke-WebRequest "$BaseUrl/LichThi/Huy/1" -Method Post -WebSession $student -SkipHttpErrorCheck -Body @{
    __RequestVerificationToken = (Token $studentPage.Content)
}
Assert-Check ($deniedPost.StatusCode -eq 403) 'Thí sinh giả POST hủy lịch bị chặn tại server'

# Một vòng tạo/sửa/hủy lịch thật trên dữ liệu demo.
$form = Invoke-WebRequest "$BaseUrl/LichThi/Create" -WebSession $staff
$option = [regex]::Match($form.Content, '<option value="([1-9][0-9]*)">#')
if (-not $option.Success) { throw 'Không có hồ sơ demo đủ điều kiện để kiểm thử' }
$begin = [DateTimeOffset]::UtcNow.ToOffset([TimeSpan]::FromHours(7)).Date.AddDays(40).AddHours(9)
$body = @{
    MaHoSo = $option.Groups[1].Value; MaLichThi = '0'; ThoiGianBatDau = $begin.ToString('yyyy-MM-ddTHH:mm');
    ThoiGianKetThuc = $begin.AddHours(1).ToString('yyyy-MM-ddTHH:mm'); HinhThucThi = '0';
    DiaDiemHoacLienKet = 'Cơ sở kiểm thử HTTP'; PhongThi = 'HTTP-' + [guid]::NewGuid().ToString('N').Substring(0,8);
    GiamThi = 'Giám thị HTTP'; GhiChu = 'Kiểm thử HTTP'; __RequestVerificationToken = (Token $form.Content)
}
$created = Invoke-WebRequest "$BaseUrl/LichThi/Create" -Method Post -WebSession $staff -Body $body
$id = [regex]::Match($created.BaseResponse.RequestMessage.RequestUri.AbsolutePath, '/Details/(\d+)$').Groups[1].Value
Assert-Check ($created.StatusCode -eq 200 -and $id) 'Tạo lịch qua form HTTP thành công'
$edit = Invoke-WebRequest "$BaseUrl/LichThi/Edit/$id" -WebSession $staff
$body.MaLichThi = $id; $body.GhiChu = 'Đã sửa qua HTTP'; $body.__RequestVerificationToken = Token $edit.Content
$edited = Invoke-WebRequest "$BaseUrl/LichThi/Edit/$id" -Method Post -WebSession $staff -Body $body
Assert-Check ([System.Net.WebUtility]::HtmlDecode($edited.Content).Contains('Đã sửa qua HTTP')) 'Sửa lịch qua HTTP thành công'
$cancelled = Invoke-WebRequest "$BaseUrl/LichThi/Huy/$id" -Method Post -WebSession $staff -Body @{ __RequestVerificationToken = (Token $edited.Content) }
Assert-Check ([System.Net.WebUtility]::HtmlDecode($cancelled.Content).Contains('Đã hủy')) 'Hủy lịch qua HTTP thành công'

# Tạo rồi sửa kết quả của một hồ sơ demo đã hoàn thành thi.
$form = Invoke-WebRequest "$BaseUrl/KetQuaThi/Create" -WebSession $staff
$option = [regex]::Match($form.Content, '<option value="([1-9][0-9]*)">#')
if (-not $option.Success) { throw 'Không có hồ sơ demo Đã thi để kiểm thử kết quả' }
$body = @{ MaHoSo = $option.Groups[1].Value; MaKetQuaThi = '0'; DiemDanhGia = '87.5'; KetQua = '0'; NhanXet = 'HTTP test'; __RequestVerificationToken = (Token $form.Content) }
$created = Invoke-WebRequest "$BaseUrl/KetQuaThi/Create" -Method Post -WebSession $staff -Body $body
$id = [regex]::Match($created.BaseResponse.RequestMessage.RequestUri.AbsolutePath, '/Details/(\d+)$').Groups[1].Value
Assert-Check ($created.StatusCode -eq 200 -and $id) 'Nhập kết quả qua HTTP thành công'
$edit = Invoke-WebRequest "$BaseUrl/KetQuaThi/Edit/$id" -WebSession $staff
$body.MaKetQuaThi = $id; $body.KetQua = '1'; $body.DiemDanhGia = '30'; $body.__RequestVerificationToken = Token $edit.Content
$edited = Invoke-WebRequest "$BaseUrl/KetQuaThi/Edit/$id" -Method Post -WebSession $staff -Body $body
Assert-Check ([System.Net.WebUtility]::HtmlDecode($edited.Content).Contains('Không đạt')) 'Sửa kết quả qua HTTP thành công'
Write-Host "SUCCESS: $script:Passed HTTP checks passed."
