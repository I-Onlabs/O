const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const TerserPlugin = require('terser-webpack-plugin');

module.exports = (env, argv) => {
  const isProduction = argv.mode === 'production';
  
  return {
    entry: './src/main.js',
    
    output: {
      path: path.resolve(__dirname, 'dist'),
      filename: isProduction ? '[name].[contenthash].js' : '[name].js',
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
            filename: 'assets/images/[name].[hash][ext]'
          }
        },
        {
          test: /\.(mp3|wav|ogg|m4a)$/,
          type: 'asset/resource',
          generator: {
            filename: 'assets/audio/[name].[hash][ext]'
          }
        },
        {
          test: /\.(woff|woff2|eot|ttf|otf)$/,
          type: 'asset/resource',
          generator: {
            filename: 'assets/fonts/[name].[hash][ext]'
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
      new CleanWebpackPlugin(),
      
      new HtmlWebpackPlugin({
        template: './index.html',
        filename: 'index.html',
        inject: 'body',
        minify: isProduction ? {
          removeComments: true,
          collapseWhitespace: true,
          removeRedundantAttributes: true,
          useShortDoctype: true,
          removeEmptyAttributes: true,
          removeStyleLinkTypeAttributes: true,
          keepClosingSlash: true,
          minifyJS: true,
          minifyCSS: true,
          minifyURLs: true
        } : false
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
    
    optimization: {
      minimize: isProduction,
      minimizer: [
        new TerserPlugin({
          terserOptions: {
            compress: {
              drop_console: isProduction,
              drop_debugger: isProduction
            },
            mangle: isProduction
          }
        })
      ],
      splitChunks: {
        chunks: 'all',
        cacheGroups: {
          vendor: {
            test: /[\\/]node_modules[\\/]/,
            name: 'vendors',
            chunks: 'all'
          }
        }
      }
    },
    
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
    
    devtool: isProduction ? 'source-map' : 'eval-source-map',
    
    performance: {
      hints: isProduction ? 'warning' : false,
      maxEntrypointSize: 512000,
      maxAssetSize: 512000
    }
  };
};