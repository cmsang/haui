# Garlic Classification System

Ứng dụng nhận diện và phân loại củ tỏi bằng xử lý ảnh và học máy, xây dựng với **WinForms + EmguCV + SVM**.

## 1. Giới thiệu

Project này được xây dựng để **phân loại củ tỏi** thành 3 nhóm:

- **Tỏi to**
- **Tỏi nhỏ**
- **Tỏi hỏng**

Hệ thống sử dụng phương pháp **phân vùng ảnh theo màu** bằng **HSV/Lab + threshold theo kênh màu** để tách củ tỏi ra khỏi nền, sau đó trích xuất đặc trưng và dùng **SVM** để nhận diện.

## 2. Mục tiêu của project

- Tự động nhận diện chất lượng củ tỏi từ ảnh đầu vào
- Hỗ trợ phân loại nhanh, giảm thao tác thủ công
- Tạo nền tảng để phát triển hệ thống kiểm tra chất lượng nông sản bằng thị giác máy tính

## 3. Công nghệ sử dụng

- **Ngôn ngữ:** C#
- **Giao diện:** WinForms
- **Thư viện xử lý ảnh:** EmguCV / OpenCV
- **Mô hình học máy:** SVM
- **Không gian màu:** HSV, Lab

## 4. Bài toán phân loại

Hệ thống nhận diện ảnh củ tỏi và phân loại thành các lớp:

- **To:** củ tỏi có kích thước lớn, đạt chuẩn
- **Nhỏ:** củ tỏi có kích thước nhỏ
- **Hỏng:** củ tỏi bị lỗi, dập, mốc, thối hoặc có dấu hiệu chất lượng kém

## 5. Quy trình xử lý tổng thể

Pipeline của hệ thống gồm các bước chính sau:

### Bước 1: Nhận ảnh đầu vào
- Ảnh được lấy từ camera hoặc chọn từ thư mục dữ liệu

### Bước 2: Tiền xử lý ảnh
- Chuyển ảnh từ RGB/BGR sang không gian màu **HSV** hoặc **Lab**
- Làm mượt ảnh để giảm nhiễu bằng Gaussian Blur hoặc Median Blur

### Bước 3: Phân vùng ảnh
- Áp dụng **threshold theo kênh màu**
- Tách vùng củ tỏi ra khỏi nền
- Dùng các phép **morphology** như Open, Close để loại bỏ nhiễu và làm đầy vùng đối tượng

### Bước 4: Trích xuất vùng quan tâm
- Tìm contour của vật thể
- Lấy vùng chứa củ tỏi
- Crop và resize ảnh về kích thước chuẩn

### Bước 5: Trích xuất đặc trưng
- Trích xuất đặc trưng hình dạng, kết cấu hoặc đặc trưng ảnh
- Có thể dùng các đặc trưng như:
  - HOG
  - Hu Moments
  - Diện tích
  - Chu vi
  - Tỉ lệ rộng / cao
  - Đặc trưng màu

### Bước 6: Phân loại bằng SVM
- Vector đặc trưng được đưa vào mô hình **SVM**
- Mô hình trả về kết quả: **To / Nhỏ / Hỏng**

## 6. Lý do chọn phương pháp HSV/Lab + SVM

### Phân vùng bằng HSV/Lab
Ưu điểm:

- Dễ tách đối tượng khỏi nền khi màu sắc khác biệt
- Nhanh, dễ triển khai trong WinForms
- Phù hợp với bài toán có điều kiện chụp tương đối ổn định

### Phân loại bằng SVM
Ưu điểm:

- Hiệu quả tốt với tập dữ liệu vừa và nhỏ
- Huấn luyện nhanh hơn so với deep learning
- Phù hợp với bài toán phân loại ảnh sau khi đã trích xuất đặc trưng

## 7. Ưu điểm của hệ thống

- Tốc độ xử lý nhanh
- Dễ triển khai trên máy tính cấu hình phổ thông
- Không cần lượng dữ liệu quá lớn
- Phù hợp cho bài toán nhận diện trong môi trường kiểm soát được ánh sáng và nền

## 8. Hạn chế

- Kết quả phân vùng phụ thuộc vào điều kiện ánh sáng
- Nếu nền quá giống màu củ tỏi thì việc tách vật thể sẽ khó hơn
- Độ chính xác của SVM phụ thuộc nhiều vào chất lượng đặc trưng đầu vào
- Chưa mạnh bằng deep learning nếu dữ liệu phức tạp hoặc môi trường thay đổi lớn
