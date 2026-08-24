***English*** | [简体中文](../../zh-CN/script-api/compress.md)

# Compress — Data Compression

Compress string data with GZip or Deflate. Input/output are base64-encoded strings so they are easy to store or
transfer in scripts.

## Compressing / Decompressing

```js
// GZip
var packed = Compress.CompressGZip('a long string...');
var original = Compress.DecompressGZip(packed);

// Deflate
var packed2 = Compress.CompressDeflate('some data');
var original2 = Compress.DecompressDeflate(packed2);
```

| Method                             | Returns  | Description                        |
|------------------------------------|----------|------------------------------------|
| `CompressGZip(text)`               | `string` | GZip compress, returns base64      |
| `DecompressGZip(base64)`           | `string` | GZip decompress, returns text      |
| `CompressDeflate(text)`            | `string` | Deflate compress, returns base64   |
| `DecompressDeflate(base64)`        | `string` | Deflate decompress, returns text   |

> The compressed output is a base64 string. An empty/null input returns an empty string.
