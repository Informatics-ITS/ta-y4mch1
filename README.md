# 🏁 Tugas Akhir (TA) - Final Project

**Nama Mahasiswa**: Widian Sasi Disertasiani

**NRP**: 5025211024

**Judul TA**: Realitas Virtual Simulasi Peran Untuk Pelatihan Perawatan Pasien Demensia

**Dosen Pembimbing**: Hadziq Fabroyir, S.Kom., Ph.D.

**Dosen Ko-pembimbing**: Dianis Wulan Sari, S.Kep., Ns., MHS., Ph.D.

---

## 📺 Demo Aplikasi

Embed video demo di bawah ini (ganti `VIDEO_ID` dengan ID video YouTube Anda):

[![Demo Aplikasi](https://i.ytimg.com/vi/zIfRMTxRaIs/maxresdefault.jpg)](https://www.youtube.com/watch?v=6CcjbA4oEqc)
*Klik gambar di atas untuk menonton demo*

---


## 🛠 Panduan Instalasi & Menjalankan Software

### 📌 Prasyarat

* Android Debug Bridge (ADB)
* Headset Pico 4
* Kabel Data Type C
* Unity (jika ingin build dari source code)

---

### ⚡ Opsi A — Instalasi Menggunakan File .APK

1. **Download file .apk ASA VR**
 
    [Link ASA VR](https://drive.google.com/file/d/1tZuZFzGt2_kRJkaq2k_cfEHbqxSWwI4R/view?usp=sharing) 
2. **Buka CMD di lokasi file ASA VR**
3. Jalankan perintah:

   ```bash
   adb install 'ASAVR.apk'
   ```
4. **Pada Pico 4:**

   * Buka daftar aplikasi yang terpasang.
   * Klik ASA VR yang muncul di daftar paling bawah.
   * Aplikasi siap digunakan.
   
   *(Pastikan Pico 4 sudah terhubung dengan kabel data)*
---

### ⚡ Opsi B — Clone Project & Build di Unity

1. **Clone repository:**

   ```bash
   git clone https://github.com/Informatics-ITS/ta-y4mch1.git
   ```
2. **Buka project di Unity Hub**

   * Pilih folder hasil clone `ta-y4mch1`
   * Tunggu hingga semua dependensi selesai di-load.
3. **Build project ke Android (.apk)**

   * Atur target platform ke Android.
   * Build dan export file `.apk`.
4. **Install .apk ke Pico 4** dengan perintah:

   ```bash
   adb install 'NamaFile.apk'
   ```

   *(Pastikan Pico 4 sudah terhubung dengan kabel data)*

---

## 📚 Dokumentasi Tambahan

### Diagram CDM Aplikasi

<img src="https://media.discordapp.net/attachments/934475107684978698/1393936164217557042/image.png?ex=6874fbb7&is=6873aa37&hm=cf1c64686104a50b5995470b11990547355ead9c799e24b1f539483069a4da2a&=&format=webp&quality=lossless" alt="CDM Aplikasi" width="600"/>

### Use Case Aplikasi

<img src="https://media.discordapp.net/attachments/934475107684978698/1393936286318071940/image.png?ex=6874fbd4&is=6873aa54&hm=b7182cfce638c7ec6f98936a9f7c8901fa02e47348e074255739b27b4e706121&=&format=webp&quality=lossless" alt="Use Case Aplikasi" width="600"/>

---

## ⁉️ Pertanyaan?

Hubungi:

* Penulis: [5025211024@student.its.ac.id](mailto:5025211024@student.its.ac.id)
* Pembimbing Utama: [hadziq@its.ac.id](mailto:hadziq@its.ac.id)
