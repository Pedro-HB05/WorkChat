const path = require("node:path");
const webpack = require("webpack");
const HtmlWebpackPlugin = require("html-webpack-plugin");

/** @type {import('webpack').Configuration} */
const config = {
  entry: "./src/index.tsx",
  output: {
    path: path.resolve(__dirname, "dist"),
    filename: "assets/[name].[contenthash].js",
    clean: true,
    publicPath: "/"
  },
  resolve: {
    extensions: [".tsx", ".ts", ".js"],
    alias: { "@": path.resolve(__dirname, "src") }
  },
  module: {
    rules: [
      { test: /\.tsx?$/, exclude: /node_modules/, use: "ts-loader" },
      { test: /\.css$/, use: ["style-loader", "css-loader"] }
    ]
  },
  plugins: [
    new HtmlWebpackPlugin({
      template: "./public/index.html",
      favicon: "./public/favicon.svg"
    }),
    new webpack.DefinePlugin({
      WORKCHAT_API_URL: JSON.stringify(
        process.env.WORKCHAT_API_URL || "https://workchat-rvup.onrender.com"
      )
    })
  ],
  devServer: {
    port: 3000,
    hot: true,
    historyApiFallback: true,
    open: true,
    proxy: [
      {
        context: ["/api", "/hubs"],
        target: "http://localhost:5019",
        changeOrigin: true,
        ws: true
      }
    ]
  },
  devtool: "source-map"
};

module.exports = config;
