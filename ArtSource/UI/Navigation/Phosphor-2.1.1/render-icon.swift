import AppKit
let source = URL(fileURLWithPath: CommandLine.arguments[1])
let target = URL(fileURLWithPath: CommandLine.arguments[2])
let svg = try String(contentsOf: source, encoding: .utf8).replacingOccurrences(of: "currentColor", with: "#ffffff")
guard let image = NSImage(data: Data(svg.utf8)) else { fatalError("Cannot decode SVG") }
let pixels = 256
let bitmap = NSBitmapImageRep(bitmapDataPlanes: nil, pixelsWide: pixels, pixelsHigh: pixels, bitsPerSample: 8, samplesPerPixel: 4, hasAlpha: true, isPlanar: false, colorSpaceName: .deviceRGB, bytesPerRow: 0, bitsPerPixel: 0)!
bitmap.size = NSSize(width: pixels, height: pixels)
NSGraphicsContext.saveGraphicsState()
NSGraphicsContext.current = NSGraphicsContext(bitmapImageRep: bitmap)
NSColor.clear.setFill()
NSRect(x: 0,y: 0,width: pixels,height: pixels).fill()
image.draw(in: NSRect(x: 0,y: 0,width: pixels,height: pixels), from: .zero, operation: .copy, fraction: 1)
NSGraphicsContext.restoreGraphicsState()
try bitmap.representation(using: .png, properties: [:])!.write(to: target)
print(target.lastPathComponent)
