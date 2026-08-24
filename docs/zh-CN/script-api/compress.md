[English](../../en-US/script-api/compress.md) | ***简体中文***

# Compress — 数据压缩

用 GZip 或 Deflate 压缩字符串数据。输入输出为 base64 编码字符串，便于在脚本中存储或传输。

## 压缩 / 解压

```js
// GZip
var packed = Compress.CompressGZip('一段很长的字符串...');
var original = Compress.DecompressGZip(packed);

// Deflate
var packed2 = Compress.CompressDeflate('some data');
var original2 = Compress.DecompressDeflate(packed2);
```

| 方法                              | 返回      | 说明                       |
|-----------------------------------|-----------|----------------------------|
| `CompressGZip(text)`              | `string`  | GZip 压缩，返回 base64     |
| `DecompressGZip(base64)`          | `string`  | GZip 解压，返回原始文本    |
| `CompressDeflate(text)`           | `string`  | Deflate 压缩，返回 base64  |
| `DecompressDeflate(base64)`       | `string`  | Deflate 解压，返回原始文本 |

> 压缩输出为 base64 字符串。空/null 输入返回空字符串。
