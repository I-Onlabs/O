const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');

module.exports = {
  mode: 'development',
  
  entry: './src/main.js',
  
  output: {
    path: path.resolve(__dirname, 'dist'),
    filename: '[name].js',
    clean: true,
    publicPath: '/'
  },
  
  module: {
    rules: [
      {
        test: /\.js$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader',
          options: {
            presets: ['@babel/preset-env']
          }
        }
      },
      {
        test: /\.css$/,
        use: ['style-loader', 'css-loader']
      },
      {
        test: /\.(png|jpg|jpeg|gif|svg|ico)$/,
        type: 'asset/resource',
        generator: {
          filename: 'assets/images/[name][ext]'
        }
      },
      {
        test: /\.(mp3|wav|ogg|m4a)$/,
        type: 'asset/resource',
        generator: {
          filename: 'assets/audio/[name][ext]'
        }
      }
    ]
  },
  
  resolve: {
    extensions: ['.js', '.json'],
    alias: {
      '@': path.resolve(__dirname, 'src'),
      '@core': path.resolve(__dirname, 'src/core'),
      '@entities': path.resolve(__dirname, 'src/entities'),
      '@systems': path.resolve(__dirname, 'src/systems'),
      '@assets': path.resolve(__dirname, 'src/assets')
    }
  },
  
  plugins: [
    new HtmlWebpackPlugin({
      template: './index.html',
      filename: 'index.html',
      inject: 'body'
    }),
    
    new CopyWebpackPlugin({
      patterns: [
        {
          from: 'README.md',
          to: 'README.md'
        },
        {
          from: 'ANGRY_DOGS_GDD.md',
          to: 'ANGRY_DOGS_GDD.md'
        }
      ]
    })
  ],
  
  devServer: {
    static: {
      directory: path.join(__dirname, 'dist')
    },
    compress: true,
    port: 8080,
    hot: true,
    open: true,
    historyApiFallback: true,
    client: {
      overlay: {
        errors: true,
        warnings: false
      }
    }
  },
  
  devtool: 'eval-source-map'
};